using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Notification;

using Epoxy;
using Epoxy.Synchronized;

using LibSasara;

using SasaraUtil.ViewModels.Utility;

namespace SasaraUtil.ViewModels.CastSplitter;

[ViewModel]
public class CastSplitterViewModel
{
	private readonly INotificationMessageManager? _notify;

	public bool IsConvertable { get; set; }
	public ObservableCollection<string> DroppedFiles { get; set; }
	public string? TargetFileName { get; set; }
	public ObservableCollection<CcsTrackViewModel> CcsTrackData { get; set; }
	public Command ResetFiles { get; }

	public Command? SaveFile { get; set; }
	public bool IsOpenWithCeVIO { get; set; }

	public CastSplitterViewModel()
	{
		DroppedFiles = new();
		CcsTrackData = new();

		ResetFiles = Command.Factory
			.CreateSync(ResetFile());

		SaveFile = CommandFactory
			.Create(SaveFileAsync);

		_notify = Utility
			.MainWindowUtil
			.GetNotifyManager();
	}

	private async ValueTask SaveFileAsync()
	{
		if(DroppedFiles is null || DroppedFiles.Count == 0){
			_notify?
				.Warn("ファイルエラー", "変換するファイルがみつかりません");
			return;
		}

		var loading = _notify?
			.Loading("Now converting...","変換しています。");

		var path = DroppedFiles.FirstOrDefault();
		if(path is null){
			_notify?
				.Warn("ファイルエラー", "変換するファイルがみつかりません");
			return;
		}

		var filter = new FileDialogFilter
		{
			Extensions = new() { "ccs" }
		};
		var fileName = Path.GetFileName(path);
		var d = new SaveFileDialog()
		{
			Title = "変換したファイルの保存先を選んでください",
			Directory =
				Path.GetDirectoryName(path),
			Filters = new(){ filter },
			InitialFileName =
				Path.ChangeExtension(fileName, "splitted.ccs"),
		};

		var saveDir = string.Empty;
		var mainWin = Utility.MainWindowUtil.GetWindow();
		if(mainWin is null){
			_notify?.Dismiss(loading!);
			return;
		}else{
			saveDir = await d.ShowAsync(mainWin);
		}

		if (saveDir is null)
		{
			//canceled
			_notify?.Dismiss(loading!);
			return;
		}

		IsConvertable = false;

		var ccs = await SasaraCcs.LoadAsync(path);
		await Models.CastSplitter.CastSplitter
			.SplitTrackByCastAsync(ccs);

		await ccs.SaveAsync(saveDir);

		_notify?.Dismiss(loading!);
		_notify?.Info("保存成功", "保存しました", true);
		IsConvertable = true;

		if(IsOpenWithCeVIO){
			Models.ProcessManager
				.Open(saveDir);
		}
	}

	public async ValueTask DropFileEventAsync(DragEventArgs e)
	{
		if(!DropUtil.IsFileAvailable(e))
		{
			IsConvertable = false;
			return;
		}

		var loading = _notify!
			.Loading("解析中", "ファイルを解析しています...");

		var list = DropUtil.GetFileNames(e);
		var list2 = await Task.Run(() =>
		{
			return list
				.Where(v =>
					Path.GetExtension(v) is ".ccs");
		});

		if(!list2.Any())
		{
			IsConvertable = false;
			_notify!.Dismiss(loading);
			return;
		}

		DroppedFiles = new(list2);
		TargetFileName = Path.GetFileName(list2.First());
		var ccs = await SasaraCcs.LoadAsync(list2.First());
		var casts = await Models.CastSplitter.CastSplitter
			.GetCastsByTrackAsync(ccs);
		var tracks = casts.Select(v => (v.Name, v.GroupId));

		var list3 = casts
			.SelectMany(v => v.Units.Select(v => v))
			.Select(v =>
				new CcsTrackViewModel(
					tracks
						.First(t => t.GroupId == v.Group)
						.Name,
					v.CastId,
					v.Text
				)
			);
		CcsTrackData = new(list3);

		IsConvertable = true;
		_notify!.Dismiss(loading);
	}

	private Action ResetFile()
	=> () =>
	{
		DroppedFiles = new();
		IsConvertable = false;
	};
}

[ViewModel]
public class CcsTrackViewModel{
	public string TrackName { get; set; }
	public string CastName { get; set; }
	public string Serif { get; set; }

	public CcsTrackViewModel(
		string trackName,
		string castName,
		string serif
	)
	{
		TrackName = trackName;
		CastName = castName;
		Serif = serif;
	}
}