using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Notification;
using Avalonia.Platform.Storage;
using Epoxy;
using Epoxy.Synchronized;

using LibSasara;
using SasaraUtil.Core.Models;
using SasaraUtil.ViewModels.Utility;

namespace SasaraUtil.ViewModels.CastSplitter;

[ViewModel]
public class CastSplitterViewModel
{
	private readonly INotificationMessageManager? _notify;
    private readonly IStorageProvider? _storage;

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

		SaveFile = Command.Factory
			.Create(SaveFileAsync);

		_notify = Utility
			.MainWindowUtil
			.GetNotifyManager();

		_storage = MainWindowUtil
			.GetWindow()?
			.StorageProvider;
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
		if(_storage is null){
			_notify?.Error("ERROR", "保存ダイアログを開けません");
			return;
		}

		var dir = await _storage
			.TryGetFolderFromPathAsync(path);
		var fileName = Path.GetFileName(path);
		var f = await _storage.SaveFilePickerAsync(new()
		{
			Title = "変換したファイルの保存先を選んでください",
			SuggestedStartLocation = dir!,
			SuggestedFileName = Path.ChangeExtension(
				fileName,
				"splitted.ccs"),
			FileTypeChoices = new FilePickerFileType[]{
				new("ccs"){Patterns = new []{"*.ccs"}}
			},
		});

		var saveDir = string.Empty;
		if(f is null){
			_notify?.Dismiss(loading!);
			return;
		}else{
			saveDir = f.Path.LocalPath;
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

		var names = await CastDataManager
			.GetCastNamesAsync(CevioCasts.Category.TextVocal);


		var list3 = casts
			.SelectMany(v => v.Units.Select(v => v))
			.Select(v =>
			{
				var n = names.FirstOrDefault(a => a.Item1 == v.CastId).Item2;
				return new CcsTrackViewModel(
					tracks
						.First(t => t.GroupId == v.Group)
						.Name,
					n,
					v.Text
				);
			}
			)
			;
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