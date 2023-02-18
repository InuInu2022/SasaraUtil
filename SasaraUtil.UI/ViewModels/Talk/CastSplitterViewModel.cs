using System.Threading.Tasks;
using System.Diagnostics;
using System;
using Epoxy;
using Epoxy.Synchronized;
using System.IO;
using Avalonia.Input;

namespace SasaraUtil.ViewModels;

[ViewModel]
public class CastSplitterViewModel
{
	public Command? OpenLicense { get; set; }

	public CastSplitterViewModel()
	{
		//
	}

	public async ValueTask DropFileEventAsync(DragEventArgs e)
	{
		//
		await Task.Delay(10);
	}
}