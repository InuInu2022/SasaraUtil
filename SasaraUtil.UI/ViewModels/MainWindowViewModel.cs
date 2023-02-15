////////////////////////////////////////////////////////////////////////////
//
// SasaraUtil
//
////////////////////////////////////////////////////////////////////////////

using System.Collections.ObjectModel;

using Epoxy;
using Epoxy.Synchronized;

using SasaraUtil.Core.Models;

namespace SasaraUtil.ViewModels;

[ViewModel]
public sealed class MainWindowViewModel
{
	public string WindowTitle { get; set; }
	public Command? Ready { get; }
	public bool IsEnabled { get; private set; }

	public MainWindowViewModel()
	{
		WindowTitle = AppUtil.GetWindowTitle();

		// A handler for window opened
		this.Ready = Command.Factory.CreateSync(() => this.IsEnabled = true);
	}
}
