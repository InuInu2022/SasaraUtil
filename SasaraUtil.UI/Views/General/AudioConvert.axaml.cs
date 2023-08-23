using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace SasaraUtil.UI.Views.General;

public partial class AudioConvert : UserControl
{
	public AudioConvert()
	{
		this.InitializeComponent();

		AddHandler(DragDrop.DropEvent, DropAsync);
	}

	async void DropAsync(object? sender, DragEventArgs e)
	{
		if (DataContext is not SasaraUtil.ViewModels
			.AudioConvertViewModel vm)
		{
			return;
		}

		await vm.DropFileEventAsync(e)
			.ConfigureAwait(false);

		/*
		var _ = await Dispatcher
			.UIThread
			.InvokeAsync(
				() => vm.DropFileEventAsync(e),
				DispatcherPriority.Background
			);
		*/
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}