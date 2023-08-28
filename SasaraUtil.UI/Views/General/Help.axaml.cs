////////////////////////////////////////////////////////////////////////////
//
// SasaraUtil
//
////////////////////////////////////////////////////////////////////////////

using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SasaraUtil.UI.Views.General;

public partial class Help : UserControl
{
    public Help()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}