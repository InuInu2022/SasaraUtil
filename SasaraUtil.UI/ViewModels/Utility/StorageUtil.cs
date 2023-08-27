using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using SasaraUtil.ViewModels.Utility;

namespace SasaraUtil.UI.ViewModels.Utility;

public static class StorageUtil
{
	private static readonly IStorageProvider? _storage =
		MainWindowUtil
			.GetWindow()?
			.StorageProvider;

	#region fileformats
	private static readonly FilePickerFileType cevioFiles =
		new("CeVIO files")
		{
			Patterns = new[] { "*.ccs", "*.ccst" },
			MimeTypes = new List<string>() { "text/xml" },
			AppleUniformTypeIdentifiers = new[] { "content" }
		};
	private static readonly FilePickerFileType cevioProjFile =
		new("CeVIO project file")
		{
			Patterns = new List<string>() { "*.ccs" },
			MimeTypes = new List<string>() { "text/xml" },
			AppleUniformTypeIdentifiers = new[] { "content" }
		};
	private static readonly FilePickerFileType cevioTrackFile =
		new("CeVIO track file")
		{
			Patterns = new List<string>() { "*.ccst" },
			MimeTypes = new List<string>() { "text/xml" },
			AppleUniformTypeIdentifiers = new[] { "content" }
		};
	#endregion

	/// <summary>
	/// open file folder dialog
	/// </summary>
	/// <returns></returns>
	public static async Task<IReadOnlyList<IStorageFile?>?> OpenCevioFileAsync(
		bool allowMultiple,
		string? path,
		string title = "開くファイルを選んでください",
		OpenCcsType openType = OpenCcsType.CssAndCsst
	)
	{
		if(_storage is null){
			return default;
		}

		FilePickerFileType[] types = openType switch
		{
			OpenCcsType.CcsOnly => new []{cevioProjFile},
			OpenCcsType.CsstOnly => new []{cevioTrackFile},
			_ => new[]{cevioFiles,cevioProjFile,cevioTrackFile}
		};

		var opt = new FilePickerOpenOptions()
		{
			Title = title,
			AllowMultiple = allowMultiple,
			FileTypeFilter = types
		};
		if(path is not null){
			opt.SuggestedStartLocation = await _storage
				.TryGetFolderFromPathAsync(path);
		}
		var f = await _storage.OpenFilePickerAsync(opt);
		return f;
	}

	/// <summary>
	/// ファイルを保存
	/// </summary>
	/// <param name="path"></param>
	/// <param name="patterns"></param>
	/// <param name="title"></param>
	/// <param name="changeExt"></param>
	/// <returns></returns>
	public static async Task<IStorageFile?> SaveAsync(
		string path,
		string[] patterns,
		string title = "保存先を選んでください",
		string changeExt = ".new.ccs"
	)
	{
		if(_storage is null){
			return default;
		}

		var dir = await _storage
			.TryGetFolderFromPathAsync(path);
		var fileName = Path.GetFileName(path);
		var f = await _storage.SaveFilePickerAsync(new()
		{
			Title = title,
			SuggestedStartLocation = dir!,
			SuggestedFileName = Path.ChangeExtension(
				fileName,
				changeExt),
			FileTypeChoices = new FilePickerFileType[]{
				new("ccs"){
					Patterns = patterns,
					AppleUniformTypeIdentifiers = new []{"content"}
				}
			},
		});
		return f;
	}
}

public enum OpenCcsType{
	CssAndCsst,
	CcsOnly,
	CsstOnly
}