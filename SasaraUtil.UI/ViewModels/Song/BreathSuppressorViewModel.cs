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
using SasaraUtil.Models.Song;
using SasaraUtil.ViewModels.Utility;

namespace SasaraUtil.ViewModels.BreathSuppressor;

[ViewModel]
public class BreathSuppressorViewModel
{
	private readonly INotificationMessageManager? _notify;

	public bool IsConvertable { get; set; }
	public ObservableCollection<string> DroppedFiles { get; set; }
	public string? TargetFileName { get; set; }
	public ObservableCollection<CcsTrackViewModel> CcsTrackData { get; set; }
	public Command ResetFiles { get; }

	public Command? SaveFile { get; set; }

	public bool IsOpenWithCeVIO { get; set; }
	public string ProjectFileName { get; private set; } = "";
	public string LabelFileName { get; private set; } = "";
	string ProjectFilePath { get; set; } = "";
	string LabelFilePath { get; set; } = "";

	public BreathSuppressorViewModel()
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
		if(ProjectFilePath is null || LabelFilePath is null){
			_notify?
				.Warn("ファイルエラー", "変換するファイルがみつかりません");
			return;
		}

		var loading = _notify?
			.Loading("Now converting...","変換しています。");

		var filter = new FileDialogFilter
		{
			Extensions = new() { "ccs","ccst" }
		};
		var fileName = Path.GetFileName(ProjectFilePath);
		var d = new SaveFileDialog()
		{
			Title = "変換したファイルの保存先を選んでください",
			Directory =
				Path.GetDirectoryName(ProjectFilePath),
			Filters = new(){ filter },
			InitialFileName =
				Path.ChangeExtension(fileName, $"suppressed{Path.GetExtension(ProjectFilePath)}"),
		};

		var savePath = string.Empty;
		var mainWin = MainWindowUtil.GetWindow();
		if(mainWin is null){
			_notify?.Dismiss(loading!);
			return;
		}else{
			savePath = await d.ShowAsync(mainWin);
		}

		if (savePath is null)
		{
			//canceled
			_notify?.Dismiss(loading!);
			return;
		}

		IsConvertable = false;

		try
		{
			var ccs = await SasaraCcs.LoadAsync(ProjectFilePath);
			var lab = await SasaraLabel.LoadAsync(LabelFilePath);

			await BreathSuppressorCore
				.SuppressAsync(
					ccs, lab, SuppressMode.Remove);
			await ccs.SaveAsync(savePath);
		}
		catch (Exception e)
		{
			_notify?.Dismiss(loading!);
			_notify?.Error("エラーが発生しました！", e.Message);
			IsConvertable = true;
			return;
		}

		_notify?.Dismiss(loading!);
		_notify?.Info("保存成功", "保存しました", true);
		IsConvertable = true;

		if(IsOpenWithCeVIO){
			Models.ProcessManager
				.Open(savePath);
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

		var list2 = list
			.Where(v => Path.GetExtension(v) is ".ccst");

		if(!list2.Any())
		{
			IsConvertable = false;
			_notify!.Dismiss(loading);
			return;
		}

		var ccsPath = list2.First();
		ProjectFilePath = ccsPath;
		ProjectFileName = Path.GetFileName(ccsPath);

		if(list.Any(v => Path.GetExtension(v) is ".lab")){
			//labファイルドロップされたら
			LabelFilePath = list.First(v => Path.GetExtension(v) is ".lab");
			LabelFileName = Path.GetFileName(LabelFilePath);
		}else if(File.Exists(Path.ChangeExtension(ccsPath, ".lab"))){
			//同名のlabファイルがあったら読む
			LabelFilePath = Path.ChangeExtension(ProjectFilePath, ".lab");
			LabelFileName = Path.GetFileName(LabelFilePath);
		}

		var ccs = await SasaraCcs.LoadAsync(ProjectFilePath);

		//ソングかどうか
		if(ccs.GetUnits(LibSasara.Model.Category.SingerSong).Count == 0){
			_notify!.Dismiss(loading);
			IsConvertable = false;
			_notify!.Warn("ソングトラックがありません", "ファイルにソングトラックがありません！");
			return;
		}

		IsConvertable = ProjectFilePath is not ""
			&& LabelFilePath is not "";
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