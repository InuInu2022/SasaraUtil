using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using FluentCeVIOWrapper.Common;

using LibSasara;
using LibSasara.Builder;
using LibSasara.Model;

using MathNet.Numerics.Statistics;

namespace SasaraUtil.Models.Talk;

/**
base source code from
https://github.com/InuInu2022/LibSasara/blob/master/sample/csharp/SongToTalk/Program.cs
**/

public class VocalPercussionCore{
	private static Process? process;
	private FluentCeVIO? fcw;
	private CeVIOFileBase? srcCcs;
	private CeVIOFileBase? exportCcs;
    private string? src;
	private string? dist;

	public async Task ExecuteAsync(
		string src,
		string dist,
		string castName,
		CeVIOFileBase template,
		string TTS,
		bool stretch,
		IEnumerable<Guid> tracks
	)
	{
		if (!File.Exists(src))
		{
			throw new FileNotFoundException($"src:{src} is not found");
		}

		this.src = src;

		/*
		if(!File.Exists(dist)){
			throw new FileNotFoundException($"{dist} is not found");
		}*/

		this.dist = dist;

		/*
		if(!string.IsNullOrEmpty(template) && !File.Exists(template)){
			throw new FileNotFoundException($"template:{template} is not found");
		}*/

		await AwakeAsync(TTS);

		fcw = await StartTalkAsync(TTS);

		var casts = await fcw.GetAvailableCastsAsync();
		await fcw.CreateParam()
			.Cast(casts[0])
			.SendAsync();

		await fcw.SpeakAsync(
			"CeVIOトークでボイパロイドします。"
		);

		if (!casts.Contains(castName))
		{
			process!.Kill();
			throw new ArgumentException($"{castName}は存在しないキャスト名です。");
		}

		await fcw.SetCastAsync(castName);
		await fcw.SpeakAsync($"キャストを{castName}に切り替えました。");

		srcCcs = await SasaraCcs.LoadAsync(src);
		await srcCcs
			.GetUnits(Category.SingerSong)
			.OfType<SongUnit>()
			.Where(u => tracks.Any(t => t == u.Group))
			.ToAsyncEnumerable()
			.ForEachAwaitAsync(async s =>
				await ToPercussionAsync(s, castName, template, stretch)
			);

		if(exportCcs is null){
			return;
		}

		exportCcs
			.GetTrackSet(new Guid("7abcbac7-a4e6-459d-a507-bf0f0bb7123a"))
			.group
			.Remove();

		await exportCcs
			.SaveAsync($"{this.dist}");
	}

	private async Task ToPercussionAsync(
		SongUnit song,
		string castName,
		CeVIOFileBase template,
		bool stretch
	)
	{
		var tempo = song.Tempo;
		var notes = song.Notes;
		var groupId = Guid.NewGuid();

		var c = await fcw!
			.GetComponentsAsync();

		var castId = await fcw.GetCastIdAsync(castName);
		//var castId = string.Join("-",c[0].Id.Split("-")[0..3]);

		//var path = string.IsNullOrEmpty(template) ?
		//	dist : template;
		exportCcs = template;//await SasaraCcs.LoadAsync(path);

		//if(Path.GetExtension(path) == "ccst"){
		//	exportCcs.RemoveAllGroups();
		//}

		exportCcs.AddGroup(
			groupId,
			Category.TextVocal,
			$"ボイパ_{castName}_{srcCcs!.GetTrackSet(song.Group).group.Attribute("Name")?.Value}"
		);

		var cacheTones = new Dictionary<string, double>();
		var cachePhonemes = new Dictionary<string, ReadOnlyCollection<FluentCeVIOWrapper.Common.Models.PhonemeData>>();

		var units = await notes
			.ToAsyncEnumerable()
			.Where(v => !string.IsNullOrEmpty(v.Lyric))
			.SelectAwait(async (v, i) =>
			{
				//"ー"
				return await ReplaceProlongedMarkAsync(v, i, notes);
			})
			.SelectAwait(async (v, i) =>
			{
				//"っ"
				return await ReplaceCloseConsonantAsync(v, i, notes);
			})
			/*.SelectAwait(async v =>
			{
				return await CulcSpeedAsync(v, tempo);
			})*/
			.SelectAwait(async v =>
			{
				//Stretch option
				//APIとccsで速さ指定単位が異なるので要計算
				//API: 0.2 ～ 5.0 (cen:1.0)
				//ccs: 0 ～ 100 (cen: 50)

				if (stretch)
				{
					//TODO: stretch
					await fcw.SetSpeedAsync((uint)50);
				}
				else
				{
					await fcw.SetSpeedAsync(50);
				}

				//await fcw.SpeakAsync(v.Lyric!);
				var culc = await fcw
					.GetTextDurationAsync(v.Lyric!);
				//Console.WriteLine($"{v.cast}[{v.Lyric}] note:{v.noteLen}, talklen:{v.len}, rate:{v.rate}, culc:{culc}");

				//pitch

				//target freq.
				var tFreq = LibSasara.SasaraUtil.OctaveStepToFreq(v.PitchOctave, v.PitchStep);
				//Console.WriteLine($"Target Freq. {tFreq}");
				await fcw.SetToneAsync(50);
				var isCachedLyric = cacheTones.ContainsKey(v.Lyric!);

				double avgFreq;
				if (isCachedLyric)
				{
					avgFreq = cacheTones[v.Lyric!];
				}
				else
				{
					var wp = await EstimateAsync(v.Lyric!);
					avgFreq = wp.F0?.Where(f => f > 0).Mean() ?? 0;
					cacheTones
						.Add(v.Lyric!, avgFreq);
				}

				Console.WriteLine($" avg:{avgFreq}, target:{tFreq}");

				var isCachedPhoneme = cachePhonemes
					.ContainsKey(v.Lyric!);
				var ph = isCachedPhoneme ?
					cachePhonemes[v.Lyric!] :
					await fcw.GetPhonemesAsync(v.Lyric!);
				if (!isCachedPhoneme)
				{
					cachePhonemes[v.Lyric!] = ph;
				}

				var phs = ph
					//.Select(p => p.Phoneme)
					.Select((p, i) =>
					{
						return new TalkPhoneme
						{
							Index = i,
							Data = p.Phoneme,
							//Speed = 0,
							Tone = TalkPhoneme.CulcTone(avgFreq, tFreq)
						};
					})
					;

				Debug.WriteLine($"phs:{phs.Count()}");

				//create units
				return TalkUnitBuilder
					.Create(
						exportCcs,
						LibSasara.SasaraUtil
							.ClockToTimeSpan(tempo, v.Clock),
						TimeSpan.FromSeconds(culc),
						castId,
						v.Lyric ?? ""
					)
					.Group(groupId)
					.Speed(1m)  //TODO:option
					.Phonemes(phs)
					.Build();

				//return (targetRate:r, setFreq);
			})
			.ToListAsync()
			;
	}

	/// <summary>
	/// 「っ」を置き換える
	/// </summary>
	/// <remarks>
	/// 「っ」単体の場合は直前の母音を調べて、先頭にくっつける。
	/// 見つからない場合は「あっ」。
	/// </remarks>
	/// <param name="v"></param>
	/// <param name="i"></param>
	/// <param name="notes"></param>
	/// <returns></returns>
	private async ValueTask<Note> ReplaceCloseConsonantAsync(Note v, int i, List<Note> notes)
	{
		//TODO:「っ」で始まる複数文字の歌詞の場合の対応
		if (v.Lyric != "っ")
		{
			return v;
		}

		var lNote = notes
			.GetRange(0, i)
			.Last(n =>
				n.Lyric is not "っ");
		var ph = await fcw!.GetPhonemesAsync(lNote.Lyric!);
		var last = ph
			.Last(v =>
				v.Phoneme is not "sil"
				and not "pau");
		var nLyric = last.Phoneme switch
		{
			"a" => "あ",
			"i" => "い",
			"u" => "う",
			"e" => "え",
			"o" => "お",
			_ => "あ"
		};
		v.Lyric = $"{nLyric}っ";
		return v;
	}

	/// <summary>
	/// 「ー」を置き換える
	/// </summary>
	/// <remarks>
	/// 直前の母音を探してその音で置き換える。
	/// もしなければ「あ」。
	/// </remarks>
	/// <param name="v"></param>
	/// <param name="i"></param>
	/// <param name="notes"></param>
	/// <returns></returns>
	private async ValueTask<Note> ReplaceProlongedMarkAsync(Note v, int i, List<Note> notes)
	{
		if (v.Lyric != "ー")
		{
			return v;
		}

		var nLyric = "あ";
		var lNote = notes
			.GetRange(0, i)
			.Last(n =>
				n.Lyric is not "ー"
					and not null
					and not "");

		var ph = await fcw!.GetPhonemesAsync(lNote.Lyric!);

		var last = ph
			.Last(v =>
				v.Phoneme is not "sil"
				and not "pau");
		nLyric = last.Phoneme switch
		{
			"a" => "あ",
			"i" => "い",
			"u" => "う",
			"e" => "え",
			"o" => "お",
			_ => "あ"
		};

		v.Lyric = nLyric;
		return v;
	}

	public static void Finish()
	{
		process?.Kill();
	}

	static async ValueTask AwakeAsync(string TTS)
	{
		var ps = new ProcessStartInfo()
		{
			FileName = Path.Combine(
				AppDomain.CurrentDomain.BaseDirectory,
				@".\server\FluentCeVIOWrapper.Server.exe"
			),
			Arguments = $"-cevio {TTS}",
			CreateNoWindow = !Core.Models.AppUtil.IsDebug,	//show console if debug,
			//ErrorDialog = true,
			UseShellExecute = false,
			//RedirectStandardOutput = true,
		};
		if(!Core.Models.AppUtil.IsDebug){
			ps!.WindowStyle = ProcessWindowStyle.Hidden;
		}

		ps.WorkingDirectory = Path.GetDirectoryName(ps.FileName);
		try
		{
			process = Process.Start(ps);
		}
		catch (Exception e)
		{
			Console.WriteLine($"Error {e}");
			throw;
		}

		System.Console.WriteLine("awaked.");
		await Task.Delay(2000);
	}

	public async Task<FluentCeVIO> StartTalkAsync(string TTS){
		var product = TTS switch
		{
			"CeVIO_AI" => Product.CeVIO_AI,
			_ => Product.CeVIO_CS
		};
		return await FluentCeVIO
			.FactoryAsync(product: product);
	}

	public async ValueTask<WorldParam> EstimateAsync(string serifText)
	{
		var tempName = Path.GetTempFileName();

		var resultOutput = await fcw!.OutputWaveToFileAsync(serifText, tempName);
		if (!resultOutput)
		{
			var msg = $"Faild to save temp file!:{tempName}";
			throw new Exception(msg);
		}

		var (fs, nbit, len, x) = await WorldUtil.ReadWavAsync(tempName);

		//Console.WriteLine((fs, nbit, len, x));
		var parameters = new WorldParam(fs);
		//ピッチ推定
		parameters = await WorldUtil.EstimateF0Async(
			x,
			len,
			parameters
		);
		if (tempName != null && File.Exists(tempName))
		{
			await Task.Run(()=> File.Delete(tempName));  //remove temp file
		}

		return parameters;
	}
}