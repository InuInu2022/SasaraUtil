using System.Threading.Tasks;
using System.Diagnostics;
using System;
using Epoxy;
using Epoxy.Synchronized;
using System.IO;

namespace SasaraUtil.ViewModels;

[ViewModel]
public class HelpViewModel
{
	public Command? OpenLicense { get; set; }

	public HelpViewModel()
	{
		OpenLicense = Command.Factory.
			Create(async ()=>{
				var path = Path.Combine(
					AppDomain.CurrentDomain.BaseDirectory,
					@"licenses\"
				);
				await Task.Run(() => Process.Start("explorer.exe", path));
			});
	}
}