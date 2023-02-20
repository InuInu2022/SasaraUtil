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
		if (!e.Data.Contains(DataFormats.FileNames))
		{
			return false;
		}

		if (e?.Data?.GetFileNames() is null)
		{
			return false;
		}

		return true;
	}

	/// <summary>
    /// ドロップされたファイルリストを返す
    /// </summary>
    /// <param name="e"></param>
    /// <returns></returns>
	public static ReadOnlyCollection<string> GetFileNames(DragEventArgs e){
		return e.Data.GetFileNames()?
			.ToList()
			.AsReadOnly()
			?? new List<string>().AsReadOnly();
	}
}