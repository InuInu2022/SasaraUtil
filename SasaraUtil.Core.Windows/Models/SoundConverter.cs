using NAudio.Wave;
using NAudio.Wave.SampleProviders;

namespace SasaraUtil.Models;

public static class SoundConverter
{
	/// <summary>
    /// CeVIOがインポート可能なフォーマット
    /// （48kHz, 16bit, モノラル）
    /// </summary>
	public static readonly WaveFormat CevioAcceptFormat
		= new(48000, 16, 1);

	public static async ValueTask<string> ConvertAsync(
		string path,
		string? outPath = null,
		bool isMonoral = false
	){
		if(!File.Exists(path)){
			throw new FileNotFoundException($"audio file path:{path} is not found!");
		}

		var outNewFile = outPath ?? Path.ChangeExtension(path, "wav");

		using var reader = new AudioFileReader(path);
		var resampler = new WdlResamplingSampleProvider(reader, CevioAcceptFormat.SampleRate);

		ISampleProvider sp = isMonoral ?
			resampler
				.ToMono()
				.ToWaveProvider16()
				.ToSampleProvider() :
			resampler
				.ToWaveProvider16()
				.ToSampleProvider() ;

		try
		{
			await Task.Run(() =>
				WaveFileWriter
					.CreateWaveFile16(
						outNewFile,
						sp
					))
				.ConfigureAwait(false);
		}
		catch
		{
			reader?.Dispose();
			throw;
		}

		return outNewFile;
	}

	/// <summary>
	/// ファイル形式がサポートしているかどうか
	/// </summary>
	/// <remarks>
	/// オーディオファイルだけでなく、動画もOK
	/// </remarks>
	public static bool IsSupportedMedia(string path){
		AudioFileReader? reader = null;
		var isSupported = true;

		try
		{
			reader = new AudioFileReader(path);
		}
		catch (FileNotFoundException)
		{
			isSupported = false;
		}
		catch
		{
			isSupported = false;
		}
		finally
		{
			reader?.Dispose();
		}

		return isSupported;
	}

	///<inheritdoc cref="IsSupportedMedia(string)"/>
	public static async ValueTask<bool> IsSupportedMediaAsync(string path){
		return await Task
			.Run(() => IsSupportedMedia(path))
			.ConfigureAwait(false);
	}
}
