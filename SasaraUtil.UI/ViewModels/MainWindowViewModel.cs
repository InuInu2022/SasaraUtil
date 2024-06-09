////////////////////////////////////////////////////////////////////////////
//
// SasaraUtil
//
////////////////////////////////////////////////////////////////////////////

using System.Collections.ObjectModel;
using Avalonia.Notification;
using Epoxy;
using Epoxy.Synchronized;

using SasaraUtil.Core.Models;
using CodingSeb.Localization;
using System.Threading.Tasks;

namespace SasaraUtil.ViewModels;

[ViewModel]
public sealed class MainWindowViewModel
{
	public string WindowTitle { get; set; }
	public Command? Ready { get; }
	public bool IsEnabled { get; private set; }

	public bool HasUpdate { get; set; }
	public bool HasAppUpdate { get; set; }
	public bool HasCastDataUpdate { get; set; }

	public INotificationMessageManager Manager { get; }
		= new NotificationMessageManager();

	public MainWindowViewModel()
	{
		WindowTitle = AppUtil.GetWindowTitle();

		// A handler for window opened
		Ready = Command.Factory.Create(async () =>
		{
			try{
				await CheckAsync()
					.ConfigureAwait(true);
			}
			catch (System.Exception)
			{
				Manager.Warn(
					Loc.Tr("Errors.UpdateCheckFailed.Header"),
					Loc.Tr("Errors.UpdateCheckFailed.Message"));
			}
			finally
			{
				HasUpdate = HasAppUpdate || HasCastDataUpdate;
			}

			this.IsEnabled = true;
		});
	}

	public async ValueTask CheckAsync()
	{
		//app update check
		HasAppUpdate = await UpdateChecker
			.Build()
			.IsAvailableAsync()
			.ConfigureAwait(true);
		//def update check
		HasCastDataUpdate = await CastDataManager
			.HasUpdateAsync()
			.ConfigureAwait(true);
	}
}
