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
using SasaraUtil.Models.Talk;
using SasaraUtil.ViewModels.Utility;
using LibSasara.Model;

namespace SasaraUtil.ViewModels.VocalPercussion;

[ViewModel]
public class VocalPercussionViewModel
{
	private readonly INotificationMessageManager? _notify;

	public bool IsConvertable { get; set; }
	public ObservableCollection<string> DroppedFiles { get; set; }
	public string? TargetFileName { get; set; }
	public ObservableCollection<CcsTrackViewModel> CcsTrackData { get; set; }
	public Command ResetFiles { get; }
	public Command? SaveFile { get; set; }
	public Command? SelectFile { get; set; }
	public bool IsOpenWithCeVIO { get; set; }
	public string DirectInputCastName { get; set; } = "さとうささら";
	public int SelectedTalkApp { get; set; }

	public ObservableCollection<TalkApps> TalkApps { get; set; }
		= new(Enum.GetValues(typeof(TalkApps)).Cast<TalkApps>());

	public VocalPercussionViewModel()
	{
		DroppedFiles = new();
		CcsTrackData = new();

		ResetFiles = Command.Factory
			.CreateSync(ResetFile());

		SaveFile = CommandFactory
			.Create(SaveFileAsync);

		SelectFile = CommandFactory
			.Create(LoadFileAsync);

		_notify = Utility
			.MainWindowUtil
			.GetNotifyManager();
	}

	private async ValueTask SaveFileAsync()
	{
		IsConvertable = false;
		if(DroppedFiles is null || DroppedFiles.Count == 0){
			_notify?
				.Warn("ファイルエラー", "変換するファイルがみつかりません");
			IsConvertable = true;
			return;
		}

		var loading = _notify?
			.Loading("Now converting...","変換しています。");

		var path = DroppedFiles.FirstOrDefault();
		if(path is null){
			_notify?
				.Warn("ファイルエラー", "変換するファイルがみつかりません");
			IsConvertable = true;
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
				Path.ChangeExtension(fileName, "voiceperc.ccs"),
		};

		var saveDir = string.Empty;
		var mainWin = Utility.MainWindowUtil.GetWindow();
		if(mainWin is null){
			_notify?.Dismiss(loading!);
			IsConvertable = true;
			return;
		}else{
			saveDir = await d.ShowAsync(mainWin);
		}

		if (saveDir is null)
		{
			//canceled
			_notify?.Dismiss(loading!);
			IsConvertable = true;
			return;
		}

		IsConvertable = false;

		//var ccs = await SasaraCcs.LoadAsync(path);
		//await Models.VocalPercussion.VocalPercussion
		//	.SplitTrackByCastAsync(ccs);

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
				DirectInputCastName,
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
				.Warn("ファイルエラー", $"変換に失敗しました。{e.Message}");
			VocalPercussionCore.Finish();
			IsConvertable = true;
			return;
		}
		finally{
			VocalPercussionCore.Finish();
		}

		_notify?.Dismiss(loading!);
		_notify?.Info("保存成功", "保存しました", true);
		IsConvertable = true;

		if(IsOpenWithCeVIO){
			Models.ProcessManager
				.Open(saveDir);
		}
	}

	private async ValueTask LoadFileAsync(){
		var filter = new FileDialogFilter
		{
			Extensions = new() { "ccs", "ccst" }
		};
		var d = new OpenFileDialog()
		{
			Title = "ボイパさせるソングデータを含むccsファイルを選んでください",
			AllowMultiple = false,
			Filters = new(){ filter },
		};

		string[]? songCcs;
		var mainWin = MainWindowUtil.GetWindow();
		if(mainWin is null){
			return;
		}else{
			songCcs = await d
				.ShowAsync(mainWin);
		}

		if (songCcs is null)
		{
			return;
		}

		await ProcessingFileAsync(new ReadOnlyCollection<string>(songCcs));
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
			.Loading("解析中", "ファイルを解析しています...");

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
		DroppedFiles = new();
		CcsTrackData = new();
		IsConvertable = false;
	};
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