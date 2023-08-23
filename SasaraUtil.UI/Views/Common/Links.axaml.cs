using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace SasaraUtil.Views.Common;

public partial class Links : UserControl
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