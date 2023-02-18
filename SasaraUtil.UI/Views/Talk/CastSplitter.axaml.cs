using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SasaraUtil.UI.Views.Talk;

public class CastSplitter : UserControl
{
    public CastSplitter()
    {
        this.InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}