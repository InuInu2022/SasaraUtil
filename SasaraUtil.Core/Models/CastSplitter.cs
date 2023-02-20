using System.Collections.ObjectModel;
using System;
using System.Linq;
using System.Threading.Tasks;
using LibSasara.Model;
using System.Collections.Generic;

namespace SasaraUtil.Models.CastSplitter;

public static class CastSplitter
{
	public static Task<ReadOnlyCollection<CastsByTrack>>
	GetCastsByTrackAsync(CeVIOFileBase ccs){
		return Task.Run(() =>
			ccs
				.GetTrackSets<TalkUnit>()
				.ConvertAll(v => new CastsByTrack(
					v.Name,
					v.GroupId,
					v.Units
				))
				.AsReadOnly()
			);
	}

	public static async ValueTask SplitTrackByCastAsync(CeVIOFileBase ccs){
		var tracks = await GetCastsByTrackAsync(ccs)
			.ConfigureAwait(false);

		//全トークユニット
		var units = tracks
			.SelectMany(v => v.Units.Select(v => v));

		//最初のキャストはトラックがあるとする
		var exists = tracks
			.Where(v => v.Units.Count > 0)
			.Select(v => (v.GroupId, v.Units[0].CastId, v.Name));

		//トークユニットでexistsにCastIdが無いものは振り分け対象
		var splited = units
			.Where(v => !exists.Any(t => t.CastId == v.CastId));

		//追加トラックと振り分けするユニットを計算
		var newtracks = new List<(string name, Guid id, Guid prevId, string castId, List<TalkUnit> units)>();
		foreach(var u in splited){
			//すでに振り分け先があるときはユニットを追加
			if(newtracks.Any(t => t.prevId == u.Group && t.castId == u.CastId)){
				var track = newtracks
					.First(t => t.prevId == u.Group && t.castId == u.CastId);
				track.units.Add(u);
				u.Group = track.id;
			}
			//振り分け先がないときはトラック追加してユニット追加
			else{
				var prevName = exists.First(e => e.GroupId == u.Group).Name;
				var newId = Guid.NewGuid();
				newtracks.Add((
					name: $"{prevName}_{u.CastId}",
					id: newId,
					prevId: u.Group,
					castId: u.CastId,
					units: new List<TalkUnit>() {u}
				));
				u.Group = newId;
			}
		}

		//ccsにトラック（Group要素追加）
		newtracks
			.ForEach(n =>
			{
				ccs.AddGroup(
					groupId: n.id,
					category: Category.TextVocal,   //talk only
					name: n.name
				);
			});
	}
}

public record struct CastsByTrack(
	string Name,
	Guid GroupId,
	List<TalkUnit> Units
)
{
	public static implicit operator (string Name, Guid GroupId, List<TalkUnit> Units)(CastsByTrack value)
	{
		return (value.Name, value.GroupId, value.Units);
	}

	public static implicit operator CastsByTrack((string Name, Guid GroupId, List<TalkUnit> Units) value)
	{
		return new CastsByTrack(value.Name, value.GroupId, value.Units);
	}
}