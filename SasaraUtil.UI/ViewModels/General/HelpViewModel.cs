using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

using Epoxy;

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