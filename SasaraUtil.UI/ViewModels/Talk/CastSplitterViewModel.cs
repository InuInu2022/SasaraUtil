using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Avalonia.Input;
using Avalonia.Notification;
using Avalonia.Platform.Storage;
using CodingSeb.Localization;
using Epoxy;
using Epoxy.Synchronized;

using LibSasara;
using SasaraUtil.Core.Models;
using SasaraUtil.UI.ViewModels.Utility;
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
				.Warn(
					$"{Loc.Tr("Errors.NotFoundFile.Header")}",
					$"{Loc.Tr("Errors.NotFoundFile.Message")}");
			return;
		}

		var loading = _notify?
			.Loading(
				$"{Loc.Tr("Notify.NowConverting.Header")}",
				$"{Loc.Tr("Notify.NowConverting.Message")}");

		var path = DroppedFiles.FirstOrDefault();
		if(path is null){
			_notify?
				.Warn(
					$"{Loc.Tr("Errors.NotFoundFile.Header")}",
					$"{Loc.Tr("Errors.NotFoundFile.Message")}");
			return;
		}
		if(_storage is null){
			_notify?.Error(
				$"{Loc.Tr("Errors.CannotOpenSaveDialog.Header")}",
				$"{Loc.Tr("Errors.CannotOpenSaveDialog.Message")}");
			return;
		}

		/*
		var dir = await _storage
			.TryGetFolderFromPathAsync(path);
		var fileName = Path.GetFileName(path);
		var f = await _storage.SaveFilePickerAsync(new()
		{
			Title = $"{Loc.Tr("Dialog.SelectConvertedFile.Title")}",
			SuggestedStartLocation = dir!,
			SuggestedFileName = Path.ChangeExtension(
				fileName,
				"splitted.ccs"),
			FileTypeChoices = [
				new("ccs"){Patterns = ["*.ccs"]}
			],
		});
		*/
		var f = await StorageUtil
			.SaveAsync(
				path,
				["*.ccs"],
				$"{Loc.Tr("Dialog.SelectConvertedFile.Title")}",
				"splitted.ccs"
			)
			.ConfigureAwait(true);

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

		var ccs = await SasaraCcs
			.LoadAsync(path)
			.ConfigureAwait(true);
		await Models.CastSplitter.CastSplitter
			.SplitTrackByCastAsync(ccs)
			.ConfigureAwait(true);

		await ccs.SaveAsync(saveDir)
			.ConfigureAwait(true);

		_notify?.Dismiss(loading!);
		_notify?.Info(
			$"{Loc.Tr("Notify.SaveSuccess.Header")}",
			$"{Loc.Tr("Notify.SaveSuccess.Message")}",
			true);
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
			.Loading(
				$"{Loc.Tr("Notify.Processing.Header")}",
				$"{Loc.Tr("Notify.Processing.Message")}");

		var list = DropUtil.GetFileNames(e);
		var list2 = await Task.Run(() =>
		{
			return list
				.Where(v =>
					Path.GetExtension(v) is ".ccs");
		}).ConfigureAwait(true);

		if(!list2.Any())
		{
			IsConvertable = false;
			_notify!.Dismiss(loading);
			return;
		}

		DroppedFiles = new(list2);
		TargetFileName = Path.GetFileName(list2.First());
		var ccs = await SasaraCcs
			.LoadAsync(list2.First())
			.ConfigureAwait(true);
		var casts = await Models.CastSplitter.CastSplitter
			.GetCastsByTrackAsync(ccs)
			.ConfigureAwait(true);
		var tracks = casts.Select(v => (v.Name, v.GroupId));

		var lang = Loc.Instance.CurrentLanguage switch
		{
			"ja" => CevioCasts.Lang.Japanese,
			"en" => CevioCasts.Lang.English,
			_ => CevioCasts.Lang.English,
		};

		var names = await CastDataManager
			.GetCastNamesAsync(
				CevioCasts.Category.TextVocal, lang)
			.ConfigureAwait(true);

		var list3 = casts
			.SelectMany(v => v.Units.Select(v => v))
			.Select(v =>
			{
				var n = names.FirstOrDefault(a => string.Equals(a.Item1, v.CastId, StringComparison.Ordinal)).Item2;
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
		DroppedFiles = [];
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