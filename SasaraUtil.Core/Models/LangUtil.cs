using System;
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

		//ReloadFiles();
	}

	public static void ReloadFiles()
	{
		//string exampleFileFileName = Path.Combine(languagesFilesDirectory, "strings.loc.json");
		//LocalizationLoader.Instance.ClearAllTranslations();
		//LocalizationLoader.Instance.AddFile(exampleFileFileName);
	}
}
