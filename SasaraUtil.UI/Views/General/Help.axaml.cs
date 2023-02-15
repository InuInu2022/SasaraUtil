using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SasaraUtil.UI.Views.General;

public class Help : UserControl
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