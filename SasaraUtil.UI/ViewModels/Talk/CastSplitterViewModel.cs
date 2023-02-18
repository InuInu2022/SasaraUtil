using System.Threading.Tasks;
using System.Diagnostics;
using System;
using Epoxy;
using Epoxy.Synchronized;
using System.IO;

namespace SasaraUtil.ViewModels;

[ViewModel]
public class CastSplitterViewModel
{
	public Command? OpenLicense { get; set; }

	public CastSplitterViewModel()
	{
		//
	}
}