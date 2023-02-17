using System.Diagnostics;
using System.IO;
using System;
using System.Threading.Tasks;
using LibSasara;
using LibSasara.Model;

namespace SasaraUtil.Models;

public static class TrackTemplateLoader
{
	static readonly string AudioTemplate
		= Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory,
			"Templates/audio_track.ccst"
		);

	static readonly string SongTemplate
		= Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory,
			"Templates/song_track.ccst"
		);

	static readonly string TalkTemplate
		= Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory,
			"Templates/talk_track.ccst"
		);

	static readonly string ProjectTemplate
		= Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory,
			"Templates/project.ccs"
		);

	public static async ValueTask<CcstTrack> LoadAudioTrackAsync()
	{
		var path = Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory,
			"Templates/audio_track.ccst"
		);
		Debug.WriteLine(path);
		return await LoadAnyTrackAsync(AudioTemplate)
			.ConfigureAwait(false);
	}

	public static async ValueTask<CcstTrack>
	LoadTalkTrackAsync()
	{
		//TODO:implemation
		throw new NotImplementedException();
		return await LoadAnyTrackAsync(TalkTemplate)
			.ConfigureAwait(false);
	}

	public static async ValueTask<CcstTrack>
	LoadSongTrackAsync()
	{
		//TODO:implemation
		throw new NotImplementedException();
		return await LoadAnyTrackAsync(SongTemplate)
			.ConfigureAwait(false);
	}

	public static async ValueTask<CcsProject>
	LoadProjectAsync()
	{
		//TODO:implemation
		throw new NotImplementedException();
		return await SasaraCcs
			.LoadAsync<CcsProject>(ProjectTemplate)
			.ConfigureAwait(false);
	}

	/// <summary>
    /// 任意のテンプレートトラックファイルを読み込む
    /// </summary>
    /// <param name="path"></param>
    /// <seealso cref="LoadAudioTrackAsync"/>
    /// <seealso cref="LoadTalkTrackAsync"/>
    /// <seealso cref="LoadSongTrackAsync"/>
    /// <exception cref="InvalidDataException"></exception>
	public static async ValueTask<CcstTrack>
	LoadAnyTrackAsync(string path)
	{
		var track = await SasaraCcs
			.LoadAsync<CcstTrack>(path)
			.ConfigureAwait(false);
		if(track is null){
			throw new InvalidDataException($"{path} is invalid!");
		}

		return track;
	}
}