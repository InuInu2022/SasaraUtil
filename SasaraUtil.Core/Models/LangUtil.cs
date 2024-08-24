using System;
using System.Globalization;
using System.IO;
using CodingSeb.Localization;
using CodingSeb.Localization.Loaders;

namespace SasaraUtil.Core.Models;

public static class LangUtil
{
	private static readonly NLog.Logger Logger
		= NLog.LogManager.GetCurrentClassLogger();
	public static void Init()
	{
		Loc.LogOutMissingTranslations = true;

		LocalizationLoader
			.Instance
			.FileLanguageLoaders
			.Add(new YamlFileLoader());
		var fileName = Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory,
			"Assets/strings.loc.yaml"
		);
		if(!File.Exists(fileName)){
			var msg = $"file {fileName} is not found.";
			Logger.Error(msg);
			throw new FileNotFoundException(msg);
		}
		LocalizationLoader
			.Instance
			.AddFile(fileName);

		var tl = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName ?? "en";
		var hasLoc = Loc.AvailableLanguages
			.Contains(tl, StringComparer.Ordinal);
		Loc.Instance.CurrentLanguage = hasLoc ? tl : "en";

		//ReloadFiles();
	}

	public static void ReloadFiles()
	{
		//string exampleFileFileName = Path.Combine(languagesFilesDirectory, "strings.loc.json");
		//LocalizationLoader.Instance.ClearAllTranslations();
		//LocalizationLoader.Instance.AddFile(exampleFileFileName);
	}
}
