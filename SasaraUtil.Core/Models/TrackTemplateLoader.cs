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
			"/Templates/audio_track.ccst"
		);

	public static async ValueTask<CcstTrack> LoadAudioTrackAsync()
	{
		var path = Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory,
			"Templates/audio_track.ccst"
		);
		Debug.WriteLine(path);
		return await LoadAnyTrackAsync(path)
			.ConfigureAwait(false);
	}

	public static async ValueTask<CcstTrack>
	LoadTalkTrackAsync()
	{
		//TODO:implemation
		throw new NotImplementedException();
		return await LoadAnyTrackAsync(AudioTemplate)
			.ConfigureAwait(false);
	}

	public static async ValueTask<CcstTrack>
	LoadSongTrackAsync()
	{
		//TODO:implemation
		throw new NotImplementedException();
		return await LoadAnyTrackAsync(AudioTemplate)
			.ConfigureAwait(false);
	}

	/// <summary>
    /// 任意のテンプレートトラックファイルを読み込む
    /// </summary>
    /// <param name="path"></param>
    /// <seealso cref="LoadAudioTrackAsync"/>
    /// <seealso cref="LoadTalkTrackAsync"/>
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