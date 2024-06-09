////////////////////////////////////////////////////////////////////////////
//
// SasaraUtil
//
////////////////////////////////////////////////////////////////////////////

using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.VisualTree;
using SasaraUtil.ViewModels;
using SasaraUtil.Views;

namespace SasaraUtil.UI.Views.General;

public partial class Home : UserControl
{
	public Home()
	{
		this.InitializeComponent();
	}

	private void InitializeComponent()
	{
		AvaloniaXamlLoader.Load(this);
	}

	protected override void OnAttachedToVisualTree(Avalonia.VisualTreeAttachmentEventArgs e)
	{
		var vm = DataContext as HomeViewModel;
		if(vm is not null)
		{
			var parent = this.FindAncestorOfType<MainWindow>();
			vm.MainVM = parent?.DataContext as MainWindowViewModel;
		}
	}
}