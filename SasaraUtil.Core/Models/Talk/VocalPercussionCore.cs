using System.Collections.ObjectModel;
using System.Diagnostics;

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

public sealed class VocalPercussionCore{
	private static Process? process;
	private FluentCeVIO? fcw;
	private CeVIOFileBase? srcCcs;
	private CeVIOFileBase? exportCcs;
    //private string? src;
	private string? dist;
	private static readonly NLog.Logger Logger
		= NLog.LogManager.GetCurrentClassLogger();

	public static bool IsShowConsole { get; set; }

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

		//this.src = src;

		this.dist = dist;

		await AwakeAsync(TTS)
			.ConfigureAwait(false);

		fcw = await StartTalkAsync(TTS)
			.ConfigureAwait(false);

		var casts = await fcw.GetAvailableCastsAsync().ConfigureAwait(false);
		await fcw.CreateParam()
			.Cast(casts[0])
			.SendAsync().ConfigureAwait(false);

		await fcw.SpeakAsync(
			"CeVIOトークでボイパロイドします。"
		).ConfigureAwait(false);

		if (!casts.Contains(castName, StringComparer.Ordinal))
		{
			Logger.Error("cast not found.");
			process!.Kill();
			throw new ArgumentException(
				$"{castName}は存在しないキャスト名です。"
				, nameof(castName));
		}
		Logger.Info($"cast: {castName}");

		await fcw.SetCastAsync(castName).ConfigureAwait(false);
		await fcw.SpeakAsync($"キャストを{castName}に切り替えました。")
			.ConfigureAwait(false);

		srcCcs = await SasaraCcs.LoadAsync(src)
			.ConfigureAwait(false);
		Logger.Info($"source file: {src}");
		await srcCcs
			.GetUnits(Category.SingerSong)
			.OfType<SongUnit>()
			.Where(u => tracks.Any(t => t == u.Group))
			.ToAsyncEnumerable()
			.ForEachAwaitAsync(async s =>
				await ToPercussionAsync(s, castName, template, stretch)
					.ConfigureAwait(false)
			)
			.ConfigureAwait(false);

		if(exportCcs is null){
			Logger.Warn("export ccs file is null");
			return;
		}

		exportCcs
			.GetTrackSet(new Guid("7abcbac7-a4e6-459d-a507-bf0f0bb7123a"))
			.group
			.Remove();

		await exportCcs
			.SaveAsync($"{this.dist}").ConfigureAwait(false);
	}

	private async Task ToPercussionAsync(
		SongUnit song,
		string castName,
		CeVIOFileBase template,
		bool stretch
	)
	{
		var tempo = song.Tempos;
		var notes = song.Notes;
		var groupId = Guid.NewGuid();

		if (fcw is null) return;

		var castId = await fcw.GetCastIdAsync(castName)
			.ConfigureAwait(false);

		exportCcs = template;
		exportCcs.AddGroup(
			groupId,
			Category.TextVocal,
			$"ボイパ_{castName}_{srcCcs!.GetTrackSet(song.Group).group.Attribute("Name")?.Value}"
		);

		var cacheTones = new Dictionary<string, double>(StringComparer.Ordinal);
		var cachePhonemes = new Dictionary<string, ReadOnlyCollection<FluentCeVIOWrapper.Common.Models.PhonemeData>>(StringComparer.Ordinal);

		var units = await notes
			.ToAsyncEnumerable()
			.Where(v => !string.IsNullOrEmpty(v.Lyric))
			.SelectAwait(async (v, i) =>
			{
				//"ー"
				return await ReplaceProlongedMarkAsync(v, i, notes).ConfigureAwait(false);
			})
			.SelectAwait(async (v, i) =>
			{
				//"っ"
				return await ReplaceCloseConsonantAsync(v, i, notes).ConfigureAwait(false);
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
					await fcw.SetSpeedAsync((uint)50).ConfigureAwait(false);
				}
				else
				{
					await fcw.SetSpeedAsync(50).ConfigureAwait(false);
				}

				var culc = await fcw
					.GetTextDurationAsync(v.Lyric!)
					.ConfigureAwait(false);

				//pitch

				//target freq.
				var tFreq = LibSasaraUtil.OctaveStepToFreq(v.PitchOctave, v.PitchStep);
				await fcw.SetToneAsync(50).ConfigureAwait(false);
				var isCachedLyric = cacheTones.ContainsKey(v.Lyric!);

				double avgFreq;
				if (isCachedLyric)
				{
					avgFreq = cacheTones[v.Lyric!];
				}
				else
				{
					var wp = await EstimateAsync(v.Lyric!).ConfigureAwait(false);
					avgFreq = wp.F0?.Where(f => f > 0).Mean() ?? 0;
					cacheTones
						.Add(v.Lyric!, avgFreq);
				}

				Console.WriteLine($" avg:{avgFreq}, target:{tFreq}");

				var isCachedPhoneme = cachePhonemes
					.ContainsKey(v.Lyric!);
				var ph = isCachedPhoneme ?
					cachePhonemes[v.Lyric!] :
					await fcw.GetPhonemesAsync(v.Lyric!).ConfigureAwait(false);
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
						LibSasaraUtil
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
			.ConfigureAwait(false);
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
		if (!string.Equals(v.Lyric, "っ", StringComparison.Ordinal))
		{
			return v;
		}

		var lNote = notes
			.GetRange(0, i)
			.Last(n =>
				n.Lyric is not "っ");
		var ph = await fcw!.GetPhonemesAsync(lNote.Lyric!).ConfigureAwait(false);
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
		if (!string.Equals(v.Lyric, "ー", StringComparison.Ordinal))
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

		var ph = await fcw!.GetPhonemesAsync(lNote.Lyric!).ConfigureAwait(false);

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

	public static async ValueTask FinishOldProcessAsync()
	{
		const string processName = "FluentCeVIOWrapper.Server.exe";
		foreach (var p in Process.GetProcessesByName(processName))
        {
            try
            {
                p.Kill();
				await p.WaitForExitAsync()
					.ConfigureAwait(false);
				var msg = $"{processName} プロセスを正常に終了しました。";
				Logger.Info(msg);
				Console.WriteLine(msg);
            }
            catch (Exception ex)
            {
				var msg = $"エラーが発生しました：{ex.Message}";
				Logger.Error(msg);
                await Console.Error
					.WriteLineAsync(msg)
					.ConfigureAwait(false);
            }
        }
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
			CreateNoWindow = !IsShowConsole, //show console,
			//ErrorDialog = true,
			UseShellExecute = IsShowConsole,
			//RedirectStandardOutput = true,
		};
		Logger.Info($"IsShowConsole: {IsShowConsole}");
		Logger.Info($"psi.CreateNoWindow: {ps.CreateNoWindow}");
		Logger.Info($"psi.UseShellExecute: {ps.UseShellExecute}");
		if(!IsShowConsole)
		{
			ps!.WindowStyle = ProcessWindowStyle.Hidden;
		}

		ps.WorkingDirectory = Path.GetDirectoryName(ps.FileName);
		try
		{
			process = Process.Start(ps);
		}
		catch (Exception e)
		{
			await Console.Error.WriteLineAsync($"Error {e}")
				.ConfigureAwait(false);
			throw;
		}

		System.Console.WriteLine("awaked.");
		await Task.Delay(2000).ConfigureAwait(false);
	}

	public async Task<FluentCeVIO> StartTalkAsync(string TTS){
		var product = TTS switch
		{
			"CeVIO_AI" => Product.CeVIO_AI,
			_ => Product.CeVIO_CS
		};
		return await FluentCeVIO
			.FactoryAsync(product: product).ConfigureAwait(false);
	}

	public async ValueTask<WorldParam> EstimateAsync(string serifText)
	{
		var tempName = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

		var resultOutput = await fcw!.OutputWaveToFileAsync(serifText, tempName).ConfigureAwait(false);
		if (!resultOutput)
		{
			var msg = $"Faild to save temp file!:{tempName}";
			throw new InvalidOperationException(msg);
		}

		var (fs, _, len, x) = await WorldUtil.ReadWavAsync(tempName).ConfigureAwait(false);

		var parameters = new WorldParam(fs);
		//ピッチ推定
		parameters = await WorldUtil.EstimateF0Async(
			x,
			len,
			parameters
		).ConfigureAwait(false);
		if (tempName != null && File.Exists(tempName))
		{
			await Task.Run(()=> File.Delete(tempName)).ConfigureAwait(false);  //remove temp file
		}

		return parameters;
	}
}