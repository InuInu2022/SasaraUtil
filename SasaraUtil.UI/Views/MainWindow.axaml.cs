////////////////////////////////////////////////////////////////////////////
//
// SasaraUtil
//
////////////////////////////////////////////////////////////////////////////

using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Chrome;
using Avalonia.Markup.Xaml;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;

namespace SasaraUtil.Views;

public sealed class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
	}

	private void InitializeComponent() =>
		AvaloniaXamlLoader.Load(this);

	// To access the CoreApplicationViewTitleBar, use the TitleBar property on the CoreWindow

	// To extend the current view into the Titlebar region, set ExtendViewIntoTitleBar to true

	protected override void OnOpened(EventArgs e)
	{
        base.OnOpened(e);

        // Default NavView
        var nv = this.FindControl<NavigationView>("SasaraUtilNav");
        nv.SelectionChanged += OnNVSample1SelectionChanged;
        nv.SelectedItem = nv.MenuItems.ElementAt(0);
		//TitleBar.ExtendViewIntoTitleBar = true;
	}

    private void OnNVSample1SelectionChanged(object? sender, NavigationViewSelectionChangedEventArgs e)
    {
        if (e.IsSettingsSelected)
        {
            //TODO
            //(sender as NavigationView).Content = new NVSamplePageSettings();
        }
        else if (e.SelectedItem is NavigationViewItem nvi)
        {
            var smpPage = $"SasaraUtil.UI.Views.{nvi.Tag}";
            if(smpPage is null){
				return;
			}

			var t = Type.GetType(smpPage);
            if(t is null){
				return;
			}

			var pg = Activator.CreateInstance(t);
            (sender as NavigationView)!.Content = pg;
        }
    }
}
