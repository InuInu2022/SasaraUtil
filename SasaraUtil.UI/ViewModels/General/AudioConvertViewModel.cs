﻿using System.IO;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics;
////////////////////////////////////////////////////////////////////////////
//
// CeVIOのオーディオ取込は16bit/48kHzのwav形式の制限があります。自動で対応形式に変換します。
//
////////////////////////////////////////////////////////////////////////////
using Epoxy;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using Avalonia.Input;
using System.Linq;
using Avalonia.Controls;
using SasaraUtil.Models;
using System;
using Avalonia.Controls.ApplicationLifetimes;
using Epoxy.Synchronized;
using LibSasara.Model;
using System.Collections.Generic;

namespace SasaraUtil.ViewModels;

[ViewModel]
public sealed class AudioConvertViewModel
{
	public Command? Ready { get; set; }
	public Command? DropFile { get; set; }
	public Command? ConvertAndSend { get; set; }
	public Command? ConvertAndSave { get; set; }
	public Command? ResetFiles { get; set; }
	public Pile<DockPanel>? DockPanelPile {get;set;}
	public ObservableCollection<AudioConvertFileListViewModel>? DroppedFiles { get; set; }
	public bool IsDropAreaVisibile { get; set; } = true;
	public bool IsProcessing { get; set; } = false;
	public bool IsConvertable { get; set; } = false;

	public AudioConvertViewModel()
	{
		DroppedFiles = new();

		ConvertAndSend = CommandFactory
			.Create(SendToCeVIO());

		ConvertAndSave = CommandFactory
			.Create(SaveFiles());

		ResetFiles = Command.Factory
			.CreateSync(ResetFile());
	}

	private Func<ValueTask> SendToCeVIO()
	=> async () =>
	{
		//TODO: support multiple files
		var path = DroppedFiles.First().Path;
		var newPath = Path.ChangeExtension(path, "16bit48khz.wav");

		await SoundConverter
			.ConvertAsync(path, newPath);

		var track = await TrackTemplateLoader
			.LoadAudioTrackAsync();

		var tc = track.GetTrackSets()[0];

		var guid = Guid.NewGuid();
		tc.SetGroupId(guid);

		var units = track.GetUnits(Category.OuterAudio);
		var u = units.Cast<AudioUnit>().First();
		u.FilePath = newPath;
		u.StartTime = new TimeSpan(0);
		//TODO: check wav length
		u.Duration = new TimeSpan(0, 1, 0);

		var tmp = Path.ChangeExtension(Path.GetTempFileName(),"ccst");

		await track.SaveAsync(tmp);

		var info = new ProcessStartInfo()
		{
			FileName = tmp,
			UseShellExecute = true
		};
		Process.Start(info);
	};

	private Func<ValueTask> SaveFiles()
	=> async () =>
	{
		if (DroppedFiles is null)
		{
			return;
		}

		var d = new OpenFolderDialog()
		{
			Title = "変換したファイルの保存先を選んでください",
			Directory =
				Directory
					.GetParent(Path.GetDirectoryName(DroppedFiles[0].Path))
					.FullName,
		};

		var saveDir = string.Empty;
		if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			saveDir = await d.ShowAsync(desktop!.MainWindow);
		}

		if (string.IsNullOrEmpty(saveDir))
		{
			//canceled
			return;
		}

		Debug.WriteLine($"save dir:{saveDir}");

		IsProcessing = true;
		IsConvertable = false;
		await Task.WhenAll((IEnumerable<Task>)DroppedFiles
			.Select(async v => await ConvertAsync(v.Path, saveDir))
		);
		Process.Start(
			"explorer.exe",
			@$"/e,/root,""{saveDir}"""
		);
		IsProcessing = false;
		IsConvertable = true;
	};

	private static async ValueTask ConvertAsync(string path, string? saveDir)
	{
		var p = Path.GetFullPath(path);
		var f = Path.GetFileName(p);
		var n = Path.Combine(
			saveDir,
			Path.ChangeExtension(f, "16bit48khz.wav")
		);
		await SoundConverter
			.ConvertAsync(p, n);
	}

	public async ValueTask DropFileEventAsync(DragEventArgs e)
	{
		if (!e.Data.Contains(DataFormats.FileNames))
		{
			IsConvertable = false;
			return;
		}

		var paths = e.Data.GetFileNames();
		if (paths is null)
		{
			IsConvertable = false;
			return;
		}

		IsProcessing = true;
		IsConvertable = false;

		await Task.Run(() =>
		{
			paths
				.ToList()
				.ForEach(v => Debug.WriteLine($"fi;e:{v}"));
		});

		var l = await Task.WhenAll(
			paths
				.ToList()
				.Select(async v =>
				{
					return (
						path: v,
						isSupport: await SoundConverter
							.IsSupportedMediaAsync(v)
					);
				})
		);
		var l2 = l
			.Where(v => v.isSupport)
			.Select(v =>
				new AudioConvertFileListViewModel(v.path));

		if(!l2.Any()){
			IsProcessing = false;
			IsConvertable = false;
			return;
		}

		DroppedFiles = new(l2);
		IsProcessing = false;
		IsConvertable = true;
	}

	private Action ResetFile()
	=> () =>
	{
		DroppedFiles = new();
		IsConvertable = false;
	};
}

[ViewModel]
public class AudioConvertFileListViewModel{
	public AudioConvertFileListViewModel(string path)
	{
		Path = path;
		FileName = System.IO.Path.GetFileName(path);
	}

	public string Path { get; set; }
	public string FileName { get; set; }
	public bool IsConvertable { get; set; }
}
