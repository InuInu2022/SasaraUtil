using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;

namespace SasaraUtil.UI.Views.Talk;

public class VocalPercussion : UserControl
{
    public VocalPercussion()
    {
        this.InitializeComponent();

        AddHandler(DragDrop.DropEvent, DropAsync);
    }

    async void DropAsync(object? sender, DragEventArgs e)
	{
		if (DataContext is not ViewModels
            .VocalPercussion
			.VocalPercussionViewModel vm)
		{
			return;
		}

		var _ = await Dispatcher
			.UIThread
			.InvokeAsync(
				() => vm.DropFileEventAsync(e),
				DispatcherPriority.Background
			);
	}

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}