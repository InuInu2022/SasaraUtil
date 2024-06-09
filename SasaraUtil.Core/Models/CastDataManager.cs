using System.Collections.ObjectModel;
using System.Diagnostics;
using CevioCasts;
using CevioCasts.UpdateChecker;

namespace SasaraUtil.Core.Models;

public static class CastDataManager
{
	[ThreadStatic]
	private static Definitions? _defs;

	public static async ValueTask<Definitions> LoadAsync()
	{
		return _defs
			?? await Task.Run(() =>
			{
				var jsonString = File.ReadAllText(
					Path.Combine(
						AppDomain.CurrentDomain.BaseDirectory,
						"Assets/data.json"
					)
				);
				_defs = Definitions.FromJson(jsonString);
				return _defs;
			}).ConfigureAwait(false);
	}

	/// <summary>
	/// Get all cast names by category and product
	/// </summary>
	/// <param name="product"></param>
	/// <param name="category"></param>
	/// <param name="lang"></param>
	/// <returns></returns>
	public static async ValueTask<ReadOnlyCollection<(string, string)>> GetCastNamesAsync(
		Product product,
		Category category,
		Lang lang = Lang.Japanese
	){
		var defs = _defs ?? await LoadAsync()
			.ConfigureAwait(false);
		if(defs is null)
		{
			return new(new List<(string, string)>());
		}

		var names = defs
			.Casts
			.Where(
				c => c.Product == product
					&& c.Category == category
			)
			.Select(v =>
				(
					id: v.Id,
					name: Array.Find(v.Names, n => n.Lang == lang)?.Display ?? "NO NAME"
				)
			)
			.ToList()
			;
		return new(names);
	}

	/// <summary>
	/// Get all cast names by a category（talk or song）
	/// </summary>
	/// <param name="category">talk or song</param>
	/// <param name="lang"></param>
	/// <returns></returns>
	public static async ValueTask<ReadOnlyCollection<(string, string)>> GetCastNamesAsync(
		Category category,
		Lang lang = Lang.Japanese
	)
	{
		var defs = _defs ?? await LoadAsync()
			.ConfigureAwait(false);
		if(defs is null)
		{
			return new(new List<(string, string)>());
		}

		var names = defs
			.Casts
			.Where(c => c.Category == category)
			.Select(v =>
				(
					id: v.Id,
					name: Array.Find(v.Names, n => n.Lang == lang)?.Display ?? "NO NAME"
				)
			)
			.ToList()
			;
		return new(names);
	}

	/// <summary>
    /// ソフトの名前から<see cref="Product"/>を返す
    /// </summary>
    /// <param name="name"></param>
    /// <returns></returns>
	public static Product ConvertProduct(string name)
	{
		return name switch
		{
			"CeVIO_AI" or "CeVIO AI"
				=> Product.CeVIO_AI,
			"CeVIO_CS" or "CeVIO CS"
				=> Product.CeVIO_CS,
			"VoiSona"
				=> Product.VoiSona,
			_ => Product.CeVIO_AI
		};
	}

	public static async ValueTask<Definitions>
	GetAllCastDefsAsync(
		CancellationToken token = default
	)
	{
		return _defs ?? await LoadAsync()
			.ConfigureAwait(false);
	}

	public static ValueTask<Definitions>
	ReloadCastDefsAsync(CancellationToken token = default)
	{
		return LoadAsync();
	}

	public static async ValueTask<bool> HasUpdateAsync(
		CancellationToken token = default
	)
	{
		var defs = await GetAllCastDefsAsync(token)
			.ConfigureAwait(false);
		var update = CevioCasts.UpdateChecker.GithubRelease
			.Build(defs);
		return await update
			.IsAvailableAsync()
			.ConfigureAwait(false);
	}

	public static async ValueTask<string> GetVersionAsync(
		CancellationToken token = default
	)
	{
		var def = await GetAllCastDefsAsync(token)
			.ConfigureAwait(false);

		return def.Version;
	}

	public static async ValueTask<string> GetRepositoryVersionAsync(
		CancellationToken token = default
	)
	{
		var defs = await GetAllCastDefsAsync(token)
			.ConfigureAwait(false);
		var update = CevioCasts.UpdateChecker.GithubRelease
			.Build(defs);
		try
		{
			var version = await update
				.GetRepositoryVersionAsync()
				.ConfigureAwait(false);
			return version.ToString();
		}
		catch (System.Exception e)
		{
			Debug.WriteLine(e.Message);
			throw;
		}
	}

	public static async ValueTask UpdateDefinitionAsync(
		IProgress<double>? progress = default,
		CancellationToken token = default
	)
	{
		var defs = await GetAllCastDefsAsync(token)
			.ConfigureAwait(false);
		var update = GithubRelease
			.Build(defs);
		var tempPath = Path.GetTempPath();
		Debug.WriteLine($"temp download: {tempPath}");
		await update
			.DownloadAsync(
				tempPath,
				percent: progress,
				cancellationToken: token
			)
			.ConfigureAwait(false);

		var destPath = Path.Combine(
			AppDomain.CurrentDomain.BaseDirectory,
			"Assets/data.json"
		);

		try
		{
			var tempStream = new FileStream(
				Path.Combine(tempPath,"data.json"),
				FileMode.Open,
				FileAccess.Read,
				FileShare.Read,
				4096,
				FileOptions.Asynchronous);
			var destStream = new FileStream(
				destPath, FileMode.Create, FileAccess.Write, FileShare.None, 4096, FileOptions.Asynchronous);
			await using (tempStream.ConfigureAwait(false))
			{
				await using (destStream.ConfigureAwait(false))
				{
					await tempStream
						.CopyToAsync(destStream, token)
						.ConfigureAwait(false);
				}
			}
		}
		catch (System.Exception e)
		{
			Debug.WriteLine($"{e.Message}");
			throw;
		}
	}
}