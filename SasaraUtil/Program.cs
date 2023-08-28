////////////////////////////////////////////////////////////////////////////
//
// Epoxy template source code.
// Write your own copyright and note.
// (You can use https://github.com/rubicon-oss/LicenseHeaderManager)
//
////////////////////////////////////////////////////////////////////////////

using System;
using Avalonia;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace SasaraUtil;

public static class Program
{
	private static readonly NLog.Logger Logger
		= NLog.LogManager.GetCurrentClassLogger();

	// Initialization code. Don't use any Avalonia, third-party APIs or any
	// SynchronizationContext-reliant code before AppMain is called: things aren't initialized
	// yet and stuff might break.
	[STAThread]
	public static void Main(string[] args)
	{
		try
		{
			InitLogger();
			Logger.Info("App starting...");
		  	BuildAvaloniaApp().
				StartWithClassicDesktopLifetime(args);
		}
		catch (Exception ex)
		{
		   Logger.Error(ex, "Main App Error!");
		}
	}

	// Avalonia configuration, don't remove; also used by visual designer.
	public static AppBuilder BuildAvaloniaApp() =>
		AppBuilder.Configure<App>()
			.UsePlatformDetect()
			.LogToTrace()
			;

	private static void InitLogger()
	{
		var config = new LoggingConfiguration();

		var fileTarget = new FileTarget();
		config.AddTarget("file", fileTarget);

		fileTarget.Name = "f";
		fileTarget.FileName = "${basedir}/logs/${shortdate}.log";
		fileTarget.Layout = "${longdate} [${uppercase:${level}}] ${message}";

		var rule1 = new LoggingRule("*", LogLevel.Info, fileTarget);
		config.LoggingRules.Add(rule1);

		LogManager.Configuration = config;
	}
}
