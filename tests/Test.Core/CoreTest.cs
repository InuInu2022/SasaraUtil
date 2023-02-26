using System.Threading.Tasks;
using System.IO;
using Xunit;
using SasaraUtil.Models;
using FluentAssertions;
using Xunit.Abstractions;
using NAudio.Wave;
using SasaraUtil.Models.CastSplitter;
using LibSasara.Model;
using LibSasara;
using SasaraUtil.Models.Song;
using System.Linq;

namespace Test.Core;

public class CoreTest
{
	private readonly ITestOutputHelper output;

    public CoreTest(ITestOutputHelper output)
    {
        this.output = output;
    }

	[Theory]
	//[InlineData("../../../assets/audio/kmasochist.mp3")]
	//[InlineData("../../../assets/audio/kmasochist.wav")]
	//[InlineData("../../../assets/audio/kmasochist_16.wav")]
	[InlineData("../../../assets/audio/test_mp4.mp4")]
	public async void ConvertTestAsync(string inputAudio)
	{
		output.WriteLine($"file path:{Path.GetFullPath(inputAudio)}");
		File.Exists(Path.GetFullPath(inputAudio))
			.Should()
			.BeTrue();

		var result = await SoundConverter
			.ConvertAsync(Path.GetFullPath(inputAudio), Path.ChangeExtension(inputAudio, ".16bit48kHz.wav"));

		result
			.Should()
			.NotBeNullOrEmpty();

		using var reader = new WaveFileReader(result);
		var format = reader.WaveFormat;

		format.BitsPerSample.Should().Be(16);
		format.Channels.Should().Be(1);
		format.SampleRate.Should().Be(48000);
		reader.Dispose();
		await Task.Delay(500);
	}

	[Theory]
	[InlineData("../../../assets/audio/kmasochist.mp3", true)]
	[InlineData("../../../assets/audio/kmasochist.wav", true)]
	[InlineData("../../../assets/audio/kmasochist_16.wav", true)]
	[InlineData("../../../assets/audio/test_text.txt", false)]
	[InlineData("../../../assets/audio/test_bitmap.bmp", false)]
	[InlineData("../../../assets/audio/test_mp4.mp4", true)]
	public async void IsSupportedAudio(string path, bool expect)
	{
		var result = SoundConverter.IsSupportedMedia(path);

		result.Should().Be(expect);

		await Task.Delay(1000);
	}

	[Theory]
	[InlineData(
		"../../../assets/ccs/splittest.ccs",
		"../../../assets/ccs/splittest.splited.ccs")]
	public async void SplitCastAsync(string path, string dist)
	{
		var ccs = await LibSasara.SasaraCcs.LoadAsync(path);
		var prevGroups = ccs.RawGroups.Count;
		await CastSplitter.SplitTrackByCastAsync(ccs);
		await ccs.SaveAsync(dist);

		var splited = await LibSasara.SasaraCcs.LoadAsync(dist);
		var groups = splited.RawGroups.Count;

		groups.Should().BeGreaterThan(prevGroups);
	}

	[Theory]
	//[InlineData(
	//	"../../../assets/ccs/breathsuppress.ccst",
	//	"../../../assets/ccs/breathsuppress.lab")]
	[InlineData(
		"../../../assets/ccs/ソング.ccst",
		"../../../assets/ccs/ソング.lab")]
	[InlineData(
		"../../../assets/ccs/ソング2.ccst",
		"../../../assets/ccs/ソング2.lab")]
	public async void BreathSuppressAsync(
		string ccsPath,
		string labPath
	)
	{
		var ccs = await SasaraCcs.LoadAsync<CeVIOFileBase>(ccsPath);
		var lab = await SasaraLabel.LoadAsync(labPath);

		//null check
		ccs.Should().NotBeNull();
		lab.Should().NotBeNull();
		if (ccs is null) { return; }

		await BreathSuppressorCore
			.SuppressAsync(
				ccs, lab, SuppressMode.Remove);

		/*ccs.GetUnits(Category.SingerSong)
			.Cast<SongUnit>()
			.First()
			.Volume
			.Length
			.Should()
			.BePositive();*/

		await ccs.SaveAsync(
			Path.ChangeExtension(ccsPath, ".spt.ccst")
		);
	}
}