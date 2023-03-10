using System;
using System.Linq;
using System.Reflection;

namespace SasaraUtil.Core.Models;

public static class AppUtil
{
    /// <summary>
    /// Debugビルドかを判定
    /// </summary>
    public static bool IsDebug =
#if DEBUG
    true;
#else
    false;
#endif

    public static string GetWindowTitle(){
        var assembly = Assembly.GetEntryAssembly().GetName();

        var versionInfo = Assembly
            .GetEntryAssembly()
            .GetCustomAttributes(typeof(AssemblyInformationalVersionAttribute))
            .Cast<AssemblyInformationalVersionAttribute>()
            .FirstOrDefault();

        return $"{assembly.Name} ver. {versionInfo.InformationalVersion}";
    }
}