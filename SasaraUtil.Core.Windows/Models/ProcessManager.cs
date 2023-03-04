using System.Diagnostics;

namespace SasaraUtil.Models;

public static class ProcessManager
{
	public static void Open(string path)
	{
		var info = new ProcessStartInfo()
		{
			FileName = path,
			UseShellExecute = true
		};
		Process.Start(info);
	}
}