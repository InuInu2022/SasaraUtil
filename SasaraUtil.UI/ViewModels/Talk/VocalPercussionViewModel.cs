using System.Collections.ObjectModel;
using Avalonia.Input;
using Avalonia.Notification;

using Epoxy;
using Epoxy.Synchronized;

using LibSasara;
using SasaraUtil.Models.Talk;
using SasaraUtil.ViewModels.Utility;
using LibSasara.Model;
using System.Diagnostics.CodeAnalysis;
using SasaraUtil.Core.Models;
using Avalonia.Platform.Storage;
using SasaraUtil.UI.ViewModels.Utility;
using CodingSeb.Localization;
using Avalonia.Controls;
using NLog;

namespace SasaraUtil.ViewModels.VocalPercussion;

[ViewModel]
public class VocalPercussionViewModel
{
	private readonly INotificationMessageManager? _notify;
    private readonly IStorageProvider? _storage;

	private static readonly NLog.Logger Logger
		= NLog.LogManager.GetCurrentClassLogger();
    private ReadOnlyCollection<(string, string)> currentCastNames
		= new(new List<(string, string)>());

	public bool IsConvertable { get; set; }
	public ObservableCollection<string> DroppedFiles { get; set; }
	public string? TargetFileName { get; set; }
	public ObservableCollection<CcsTrackViewModel> CcsTrackData { get; set; }
	public Command ResetFiles { get; }
	public Command? SaveFile { get; set; }
	public Command? SelectFile { get; set; }
	public Command? Ready { get; set; }
	public Command? ResetServer { get; set; }

	public Well DockPanelWell { get; }
		= Well.Factory.Create<DockPanel>();
	public bool IsOpenWithCeVIO { get; set; }
	public int SelectedTalkApp { get; set; } = -1;

	public ObservableCollection<TalkApps> TalkApps { get; set; }

	public int SelectedTalkCast { get; set; }

	public ObservableCollection<string> TalkCasts { get; set; }
		= [];

	public bool IsShowConsole
	{
		get => VocalPercussionCore.IsShowConsole;
		set => VocalPercussionCore.IsShowConsole = value;
	}

	private static readonly string[] patterns = ["*.ccs"];

	public VocalPercussionViewModel()
	{
		DroppedFiles = [];
		CcsTrackData = [];
		TalkApps = new(Enum.GetValues(typeof(TalkApps)).Cast<TalkApps>());
		SelectedTalkApp = 0;

		DockPanelWell.Add(DragDrop.DropEvent, DropFileEventAsync);

		ResetFiles = Command.Factory
			.CreateSync(ResetFile());

		SaveFile = Command.Factory
			.Create(SaveFileAsync);

		SelectFile = Command.Factory
			.Create(LoadFileAsync);

		ResetServer = Command.Factory
			.Create(ResetServerAsync);

		_notify = Utility
			.MainWindowUtil
			.GetNotifyManager();

		_storage = MainWindowUtil
			.GetWindow()?
			.StorageProvider;
	}

	private async ValueTask ResetServerAsync()
	{
		VocalPercussionCore.Finish();
		await VocalPercussionCore
			.FinishOldProcessAsync()
			.ConfigureAwait(false);
		await Task.CompletedTask
			.ConfigureAwait(false);
	}

	private async ValueTask SaveFileAsync()
	{
		IsConvertable = false;
		if(DroppedFiles is null || DroppedFiles.Count == 0){
			_notify?
				.Warn(
					$"{Loc.Tr("Errors.NotFoundFile.Header")}",
					$"{Loc.Tr("Errors.NotFoundFile.Message")}");
			IsConvertable = true;
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
			IsConvertable = true;
			return;
		}
		if(_storage is null){
			_notify?.Error(
				$"{Loc.Tr("Errors.CannotOpenSaveDialog.Header")}",
				$"{Loc.Tr("Errors.CannotOpenSaveDialog.Message")}");
			return;
		}

		var f = await StorageUtil.SaveAsync(
			path,
			patterns,
			$"{Loc.Tr("Dialog.SelectConvertedFile.Title")}",
			"voiceperc.ccs"
		);

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
			IsConvertable = true;
			return;
		}

		IsConvertable = false;

		var vp = new VocalPercussionCore();
		var template = await Models.TrackTemplateLoader.LoadProjectAsync();
		var tracks = CcsTrackData
			.Where(v => v.IsConvert)
			.Select(v => v.Id)
			.ToList();

		try
		{
			await vp.ExecuteAsync(
				path,
				saveDir,
				TalkCasts[SelectedTalkCast],
				template,
				TalkApps[SelectedTalkApp].ToString(),
				false,
				tracks:tracks
			);
		}
		catch (Exception e)
		{
			_notify?.Dismiss(loading!);
			_notify?
				.Warn(
					$"{Loc.Tr("Errors.CannotConvert.Header")}",
					$"{Loc.Tr("Errors.CannotConvert.Message")} {e.Message}");
			VocalPercussionCore.Finish();
			IsConvertable = true;
			return;
		}
		finally{
			VocalPercussionCore.Finish();
		}

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

	private async ValueTask LoadFileAsync(){
		var songCcs = await StorageUtil.OpenCevioFileAsync(
			title:$"{Loc.Tr("Dialog.SelectOpenCeVIOFile.Title")}",
			allowMultiple: false,
			path:null
		);

		if (songCcs is null || songCcs.Count == 0)
		{
			return;
		}

		var list = songCcs
			.Where(v => v is not null)
			.Select(v => v!.Path.LocalPath)
			.ToList()
			;

		await ProcessingFileAsync(new ReadOnlyCollection<string>(list));
	}

	public async ValueTask DropFileEventAsync(DragEventArgs e)
	{
		if(!DropUtil.IsFileAvailable(e))
		{
			IsConvertable = false;
			return;
		}

		await ProcessingFileAsync(DropUtil.GetFileNames(e));
	}

	private async ValueTask ProcessingFileAsync(
		ReadOnlyCollection<string> pathes
	)
	{
		var loading = _notify!
			.Loading(
				$"{Loc.Tr("Notify.Processing.Header")}",
				$"{Loc.Tr("Notify.Processing.Message")}");

		var list = pathes;
		var list2 = await Task.Run(() =>
		{
			return list
				.Where(v =>
					Path.GetExtension(v) is ".ccs" or
					".ccst");
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

		//TODO:here
		ccs
			.GetUnits(Category.SingerSong)
			.Cast<SongUnit>()
			.ToList()
			.ForEach(u =>
			{
				var track = ccs
					.GetTrackSet<SongUnit>(u.Group);
				var name = track.Name;
				var cast = track.CastId;
				var lyrics = string.
					Join(
						" ",
						u.Notes
							.Select(n => n.Lyric ?? "")
					);
				CcsTrackData.Add(new(
					name,
					cast,
					lyrics,
					false,
					id: track.GroupId
				));
			});

		IsConvertable = true;
		_notify!.Dismiss(loading);
	}

	private Action ResetFile()
	=> () =>
	{
		DroppedFiles = [];
		CcsTrackData = [];
		IsConvertable = false;
	};

	[PropertyChanged(nameof(SelectedTalkApp))]
	[SuppressMessage("Usage", "IDE0051")]
	private async ValueTask SelectedTalkAppChangedAsync(int value)
	{
		if(value < 0)return;

		var lang = Loc.Instance.CurrentLanguage switch
		{
			"ja" => CevioCasts.Lang.Japanese,
			"en" => CevioCasts.Lang.English,
			_ => CevioCasts.Lang.English,
		};

		currentCastNames = await CastDataManager
			.GetCastNamesAsync(
				CastDataManager.ConvertProduct(TalkApps[value].ToString()),
				CevioCasts.Category.TextVocal,
				lang
			);
		var names = currentCastNames
			.Select(v => v.Item2);

		TalkCasts = new(names);
		SelectedTalkCast = 0;
	}

	[PropertyChanged(nameof(SelectedTalkCast))]
	[SuppressMessage("Usage", "IDE0051")]
	private ValueTask SelectedTalkCastChangedAsync(int value)
	{
		return new ValueTask();
	}
}

public enum TalkApps{
	CeVIO_AI,
	CeVIO_CS,
}

[ViewModel]
public class CcsTrackViewModel{
	public string TrackName { get; set; }
	public string CastName { get; set; }
	public string Serif { get; set; }
	public bool IsConvert { get; set; }
	public Guid Id { get; set; }

	public CcsTrackViewModel(
		string trackName,
		string castName,
		string serif,
		bool isConvert,
		Guid id
	)
	{
		TrackName = trackName;
		CastName = castName;
		Serif = serif;
		IsConvert = isConvert;
		Id = id;
	}
}