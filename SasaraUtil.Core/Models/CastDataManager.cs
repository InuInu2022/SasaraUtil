using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CevioCasts;

namespace SasaraUtil.Core.Models;

public static class CastDataManager
{
	private static Definitions? _defs;

	public static async ValueTask<Definitions?> LoadAsync()
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
			});
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
		var defs = _defs ?? await LoadAsync();
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
					name: Array.Find(v.Names, n => n.Lang == lang).Display
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
		var defs = _defs ?? await LoadAsync();
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
					name: Array.Find(v.Names, n => n.Lang == lang).Display
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
}