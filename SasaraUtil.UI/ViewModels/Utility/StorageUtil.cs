using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Platform.Storage;
using SasaraUtil.ViewModels.Utility;

namespace SasaraUtil.UI.ViewModels.Utility;

public class StorageUtil
{
	private static IStorageProvider? _storage =
		MainWindowUtil
			.GetWindow()?
			.StorageProvider;

	/// <summary>
	///
	/// </summary>
	/// <returns></returns>
	public static async Task<IReadOnlyList<IStorageFile?>?> OpenAsync(
		bool allowMultiple,
		string[] patterns,
		string? path,
		string title = "開くファイルを選んでください"
	)
	{
		if(_storage is null){
			return default;
		}

		var opt = new FilePickerOpenOptions()
		{
			Title = title,
			AllowMultiple = allowMultiple,
			FileTypeFilter = new FilePickerFileType[]{
				new("ccs"){
					Patterns = patterns,
					AppleUniformTypeIdentifiers = new []{"content"}
				}
			},
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