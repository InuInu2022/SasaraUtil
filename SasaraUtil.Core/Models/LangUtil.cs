using System;
using System.Globalization;
using System.IO;
using CodingSeb.Localization;
using CodingSeb.Localization.Loaders;

namespace SasaraUtil.Core.Models;

public static class LangUtil
{
	public static void Init()
	{
		Loc.LogOutMissingTranslations = true;

		LocalizationLoader
			.Instance
			.FileLanguageLoaders
			.Add(new YamlFileLoader());
		LocalizationLoader
			.Instance
			.AddFile(
				Path.Combine(
					AppDomain.CurrentDomain.BaseDirectory,
					"Assets/strings.loc.yaml"
				)
			);

		var tl = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName ?? "en";
		var hasLoc = Loc.AvailableLanguages.Contains(tl);
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
