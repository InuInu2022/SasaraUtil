using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

namespace SasaraUtil.UI.Views.Talk;

public partial class CastSplitter : UserControl
{
	public CastSplitter()
	{
		this.InitializeComponent();

		//AddHandler(DragDrop.DropEvent, DropAsync);
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
			.ConfigureAwait(true);
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}