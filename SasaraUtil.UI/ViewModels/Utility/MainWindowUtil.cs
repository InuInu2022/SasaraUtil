using System;
using System.Runtime.Versioning;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Notification;
using Epoxy.Infrastructure;

namespace SasaraUtil.ViewModels.Utility;

public static class MainWindowUtil
{
	/// <summary>
    /// デスクトップアプリの時、<c>MainWindow</c>を返します
    /// </summary>
    /// <returns></returns>
	public static Window? GetWindow(){
		if (Avalonia.Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
		{
			return desktop.MainWindow;
		}
		else{
			return default;
		}
	}

	/// <summary>
    /// Epoxyでinjectされた<see cref="ViewModelBase"/>を継承するViewModelを返します
    /// </summary>
    /// <returns></returns>
	public static T? GetEpoxyViewModel<T>()
	{
		return (T?)GetWindow()?.DataContext;
	}

	/// <summary>
    /// MainWindowの<see cref="INotificationMessageManager"/>を返します
    /// </summary>
    /// <seealso cref="NotifyUtil"/>
    /// <returns></returns>
	public static INotificationMessageManager?
	GetNotifyManager() =>
		GetEpoxyViewModel<MainWindowViewModel>()?
			.Manager;
}