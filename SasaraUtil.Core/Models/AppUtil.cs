using System.Linq;
using System.Reflection;

namespace SasaraUtil.Core.Models;

public static class AppUtil
{
	/// <summary>
	/// Debugビルドかを判定
	/// </summary>
#pragma warning disable CA2211 // 非定数フィールドは表示されません
	public static bool IsDebug =
#if DEBUG
	true;
#else
	false;
#endif
#pragma warning restore CA2211 // 非定数フィールドは表示されません

	public static string GetWindowTitle(){
		var assembly = Assembly.GetEntryAssembly()?.GetName();

		var versionInfo = Assembly
			.GetEntryAssembly()?
			.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute))
			.Cast<AssemblyInformationalVersionAttribute>()
			.FirstOrDefault();

		var pkgVer = versionInfo?.InformationalVersion.Split('+')[0];
		return $"{assembly?.Name} ver. {pkgVer}";
	}

	public static string GetAppVer(){
		return Assembly
			.GetEntryAssembly()?
			.GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute))
			.Cast<AssemblyInformationalVersionAttribute>()
			.FirstOrDefault()?
			.InformationalVersion ?? string.Empty;
	}
}