using System.Collections.ObjectModel;
using Avalonia.Input;

namespace SasaraUtil.ViewModels.Utility;

public static class DropUtil
{
	/// <summary>
	/// Dropイベントにファイルがあるかどうか
	/// </summary>
	/// <param name="e"></param>
	public static bool IsFileAvailable(DragEventArgs e){
		if (e?.Data?.GetFiles() is null) return false;

		return e?.Data?.GetFiles() is { } files && files.Any();
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