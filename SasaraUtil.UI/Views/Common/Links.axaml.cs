using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SasaraUtil.Views.Common;

public class Links : UserControl
{
    public Links()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}