using Epoxy;
using SasaraUtil.Core.Models;

namespace SasaraUtil.ViewModels;

[ViewModel]
public class HomeViewModel
{
	public MainWindowViewModel? MainVM { get; set; }
	public string AppVer { get; set; }
		= "ver." + AppUtil.GetAppVer();
	public string LatestAppVer { get; set; } = "0.0.0";
	public string CastDataVersion { get; set; } = "0.0.0";
	public string LatestCastDataVersion { get; set; } = "0.0.0";

	public Command? Ready { get; private set; }
	public Command? DownloadCastData { get; set; }
	public string AppDownloadPath { get; set; } = "";
	public DownloadProgress DlProgress { get; private set; }

	public HomeViewModel()
	{
		Ready = Command.Factory.Create(GetReady());
		DownloadCastData = Command.Factory.Create(SetDownloadCastDataEvent());
	}

	private Func<ValueTask> GetReady()
	{
		return async () =>
		{
			CastDataVersion = "ver. " + await CastDataManager
				.GetVersionAsync()
				.ConfigureAwait(true);
			try
			{
				LatestCastDataVersion = "ver. " + await CastDataManager
					.GetRepositoryVersionAsync()
					.ConfigureAwait(true);

				var update = UpdateChecker.Build();
				LatestAppVer = await update
					.GetRepositoryVersionAsync()
					.ConfigureAwait(true)
				;
				AppDownloadPath = await update
					.GetDownloadUrlAsync()
					.ConfigureAwait(true);
			}
			catch (Exception ex)
			{
				var notify = MainVM?.Manager;
				notify?.Warn("Update check failed.",$"更新を確認できませんでした。ネットワーク接続を確認するかキャストデータを確認してください。 {ex.Message}");
			}
		};
	}

	private Func<ValueTask> SetDownloadCastDataEvent()
	{
		return async () =>
		{
			var notify = MainVM?.Manager;
			DlProgress = new DownloadProgress();

			var loading = notify?
				.Loading(
					"Cast data downloading...",
					"ボイスライブラリデータを更新しています。"//,
					//progress: DlProgress,
					//isIndeterminate: true
					);

			await CastDataManager
				.UpdateDefinitionAsync(DlProgress)
				.ConfigureAwait(true);

			await CastDataManager
				.ReloadCastDefsAsync()
				.ConfigureAwait(true);

			if (MainVM is not null)
			{
				MainVM.HasCastDataUpdate = false;
				await MainVM
					.CheckAsync()
					.ConfigureAwait(true);
				MainVM.HasUpdate = MainVM.HasAppUpdate;
				/*await MainVM
					.LoadCastDataAsync(true)
					.ConfigureAwait(true);
				*/
			}

			notify?.Dismiss(loading!);
		};
	}


}

public sealed class DownloadProgress : IProgress<double>
{
	public double Value { get; private set; }
	public void Report(double value)
	{
		Value = value;
	}
}