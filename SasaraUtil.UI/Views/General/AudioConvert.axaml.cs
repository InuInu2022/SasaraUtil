////////////////////////////////////////////////////////////////////////////
//
// SasaraUtil
//
////////////////////////////////////////////////////////////////////////////

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;

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
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}
}