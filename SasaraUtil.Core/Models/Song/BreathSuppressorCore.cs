using System;
using LibSasara.Model;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using LibSasara.Model.Serialize;
using System.Collections.Generic;

namespace SasaraUtil.Models.Song;

public static class BreathSuppressorCore
{
	const double VOL_ZERO = -2.4;
	const double INDEX_SPAN_TIME = 0.005;
	static readonly Regex reg =
		new("pau|sil", RegexOptions.Compiled);

	public static async ValueTask SuppressAsync(
		CeVIOFileBase project,
		Lab lab,
		SuppressMode mode = SuppressMode.Remove
	)
	{
		switch (mode)
		{
			case SuppressMode.Remove:
			{
				await RemoveAsync(project, lab)
					.ConfigureAwait(false);
				break;
			}

			case SuppressMode.Suppress:
			{
				await SuppressVolumeAsync(project, lab)
					.ConfigureAwait(false);
				break;
			}

			default:
				throw new NotSupportedException("そのモードは対応していません。");
		}
	}

	static async ValueTask RemoveAsync(
		CeVIOFileBase project,
		Lab lab
	)
	{
		//labからブレス部分のみを抜き出し
		var noSounds = await Task.Run(() =>
			lab.Lines
				.AsParallel()
				.Where(v => v is not null
					&& reg.IsMatch(v.Phoneme))
				.ToList()
		);

		//ccsのC0の該当部分のVOLを削る
		var su = project
			.GetUnits(Category.SingerSong)
			.Cast<SongUnit>()
			.FirstOrDefault();

		if(su is null){
			throw new Exception("Song track not found! ソングトラックがありません。");
		}

		//長さを求める
		var total = su.Duration.TotalSeconds;
		var totalLen = Math.Max(
			Convert.ToInt32(total / INDEX_SPAN_TIME),
			su.Volume.Length
		);
		su.Volume.Length = totalLen;

		var fullVol = su.Volume
			.GetFullData()
			;

		var nsList = noSounds
			.AsParallel()
			.Select(v =>
			{
				var sSec = v.From / 10000000;
				var eSec = v.To / 10000000;
				//var len = eSec - sSec;
				return (From:sSec, To:eSec);
			})
			.ToList();

		var full2 = await fullVol
			.ToAsyncEnumerable()
			.SelectAwait(async v =>
			{
				var isInner = await IsInNoVoiceAsync(nsList, v);

				if(isInner){
					var n = new Data
					{
						Value = Convert.ToDecimal(VOL_ZERO),
						Index = v.Index,
						Repeat = v.Repeat
					};
					return n;
				}

				return v;
			})
			.ToListAsync()
			;

		//上書き！
		var newVol = su.Volume;
		newVol.Data = full2
			.Cast<TuneData>()
			.ToList();
		newVol.Length = full2.Count;
		su.Volume = newVol;
	}

	static async ValueTask<bool> IsInNoVoiceAsync(
		List<(double From, double To)> nsList,
		ITuneData data
	){
		var time = data.Index * INDEX_SPAN_TIME;
		return await Task.Run(() =>
			nsList
				.Any(v => v.From <= time && time <= v.To)
		);
	}

	static async ValueTask SuppressVolumeAsync(
		CeVIOFileBase project,
		Lab lab
	){
		//TODO:implementaion
		throw new NotImplementedException();
	}
}

public enum SuppressMode
{
	Remove,
	Suppress
}