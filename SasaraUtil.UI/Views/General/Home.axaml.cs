////////////////////////////////////////////////////////////////////////////
//
// SasaraUtil
//
////////////////////////////////////////////////////////////////////////////

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

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
}