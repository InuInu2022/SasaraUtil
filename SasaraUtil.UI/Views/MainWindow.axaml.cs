////////////////////////////////////////////////////////////////////////////
//
// SasaraUtil
//
////////////////////////////////////////////////////////////////////////////

using System;
using Avalonia.Controls;
using FluentAvalonia.Core;
using FluentAvalonia.UI.Controls;

namespace SasaraUtil.Views;

public sealed partial class MainWindow : Window
{
	public MainWindow()
	{
		InitializeComponent();
	}

	// To access the CoreApplicationViewTitleBar, use the TitleBar property on the CoreWindow

	// To extend the current view into the Titlebar region, set ExtendViewIntoTitleBar to true

	protected override void OnOpened(EventArgs e)
	{
		base.OnOpened(e);

		// Default NavView
		var nv = this.FindControl<NavigationView>("SasaraUtilNav");
		if (nv is null) return;
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
			if (smpPage is null)
			{
				return;
			}

			var t = Type.GetType(smpPage);
			if (t is null)
			{
				return;
			}

			var pg = Activator.CreateInstance(t);
			(sender as NavigationView)!.Content = pg;
		}
	}
}
