using System.Linq;
using System.Collections.ObjectModel;
using System;
using Avalonia.Input;
using System.Collections.Generic;

namespace SasaraUtil.ViewModels.Utility;

public static class DropUtil
{
	/// <summary>
	/// Dropイベントにファイルがあるかどうか
	/// </summary>
	/// <param name="e"></param>
	public static bool IsFileAvailable(DragEventArgs e){
		if (e?.Data?.GetFiles() is null) return false;
		else if (e?.Data?.GetFiles() is {} files && files.Any()) return true;
		//if (e?.Data?.GetFiles() is not {} files || files.Any())
		//{
		//	return false;
		//}

		return false;
	}

	/// <summary>
    /// ドロップされたファイルリストを返す
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
	public static ReadOnlyCollection<string> GetFileNames(DragEventArgs e){
		return e.Data.GetFiles()?
			.Select(v => v.Path.LocalPath)
			.ToList()
			.AsReadOnly()
			?? new List<string>().AsReadOnly();
	}
}