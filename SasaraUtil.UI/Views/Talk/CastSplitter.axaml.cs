using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace SasaraUtil.UI.Views.Talk;

public partial class CastSplitter : UserControl
{
	public CastSplitter()
	{
		this.InitializeComponent();

		AddHandler(DragDrop.DropEvent, DropAsync);
	}

	async void DropAsync(object? sender, DragEventArgs e)
	{
		if (DataContext is not SasaraUtil.ViewModels
			.CastSplitter
			.CastSplitterViewModel vm)
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
				DispatcherPriority.SystemIdle
			);
		*/
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}