using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;

using Modelbouwer.Model;

namespace Modelbouwer.Services;

public enum SupplierRowAction
{
	InsertNew,
	UpdateExisting
}

public sealed class CornwallDowelImportRunResult
{
	public List<CornwallDowelImportItem> Items { get; } = [];
	public int CreatedProducts { get; set; }
	public int ReusedProducts { get; set; }
	public int CreatedSupplierRows { get; set; }
	public int UpdatedSupplierRows { get; set; }
}

public class CornwallDowelImportService
{
	private const string RootPageUrl = "https://www.cornwallmodelboats.co.uk/acatalog/Model-Boat-Timber-Wood-Dowels.html";
	private const string AcatalogBaseUrl = "https://www.cornwallmodelboats.co.uk/acatalog/";

	private static readonly string[] ExpectedSubPages =
	[
		"Balsa_Dowel.html",
		"birch-dowel.html",
		"lime_dowel.html",
		"Sapelli-Dowel.html",
		"walnut_dowel.html"
	];

	private static readonly Regex ProductAnchorRegex = new(
		"<a class=\"product-name\" href=\"([^\"]+)\">(.*?)</a>",
		RegexOptions.Singleline | RegexOptions.Compiled );

	private static readonly Regex HeadingRegex = new(
		"<h2>(.*?)</h2>",
		RegexOptions.Singleline | RegexOptions.Compiled );

	private static readonly Regex ImageRegex = new(
		"<img[^>]+src=\"([^\"]+)\"[^>]+title=\"([^\"]*)\"",
		RegexOptions.Singleline | RegexOptions.Compiled );

	private static readonly Regex PriceRegex = new(
		"price-num\">&nbsp;.(\\d[0-9,\\.]*)\\s*</span>",
		RegexOptions.Singleline | RegexOptions.Compiled );

	private static readonly Regex DowelLinkRegex = new(
		"href=\"([^\"]*(?:Dowel|dowel|Dowl|dowl)[^\"]*\\.html)\"",
		RegexOptions.Singleline | RegexOptions.Compiled );

	private static readonly Regex DimensionsRegex = new(
		@"(\d+)\s*mm\s*x\s*(\d+)\s*mm",
		RegexOptions.IgnoreCase | RegexOptions.Compiled );

	private readonly HttpClient _httpClient;

	public CornwallDowelImportService()
		: this( CreateHttpClient() )
	{
	}

	public CornwallDowelImportService( HttpClient httpClient )
	{
		_httpClient = httpClient;
	}

	public static string BuildProductCode( string productName, ISet<string> existingCodes )
	{
		ArgumentException.ThrowIfNullOrWhiteSpace( productName );
		ArgumentNullException.ThrowIfNull( existingCodes );

		var materialCode = GetMaterialCode( productName );
		var (diameter, length) = ParseDimensions( productName );
		var baseCode = $"CWD-{materialCode}-{diameter}X{length}";

		if ( !existingCodes.Contains( baseCode ) )
		{
			return baseCode;
		}

		var suffix = 2;
		while ( existingCodes.Contains( $"{baseCode}-{suffix}" ) )
		{
			suffix++;
		}

		return $"{baseCode}-{suffix}";
	}

	public static SupplierRowAction DetermineSupplierRowAction( ProductSupplierModel? existingRow )
	{
		return existingRow?.ProductSupplierId > 0
			? SupplierRowAction.UpdateExisting
			: SupplierRowAction.InsertNew;
	}

	public async Task<IReadOnlyList<CornwallDowelImportItem>> ScrapeItemsAsync( CancellationToken cancellationToken = default )
	{
		var rootHtml = await GetHtmlAsync( RootPageUrl, cancellationToken );
		var subPageUrls = ExtractSubPageUrls( rootHtml );

		var items = new List<CornwallDowelImportItem>();
		foreach ( var subPageUrl in subPageUrls )
		{
			var pageHtml = await GetHtmlAsync( subPageUrl, cancellationToken );
			items.AddRange( ParseProductPage( pageHtml ) );
		}

		return items;
	}

	public async Task<byte[]?> DownloadImageAsync( string absoluteImageUrl, CancellationToken cancellationToken = default )
	{
		if ( string.IsNullOrWhiteSpace( absoluteImageUrl ) )
		{
			return null;
		}

		using var response = await _httpClient.GetAsync( absoluteImageUrl, cancellationToken );
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadAsByteArrayAsync( cancellationToken );
	}

	public async Task<CornwallDowelImportRunResult> RunImportAsync(
		IProductService productService,
		ISupplierService supplierService,
		CornwallDowelImportDefaults defaults,
		int supplierId,
		CancellationToken cancellationToken = default )
	{
		ArgumentNullException.ThrowIfNull( productService );
		ArgumentNullException.ThrowIfNull( supplierService );
		ArgumentNullException.ThrowIfNull( defaults );

		var supplier = ( await supplierService.GetAllSuppliersAsync() )
			.FirstOrDefault( s => s.Id == supplierId )
			?? throw new InvalidOperationException( $"Supplier {supplierId} was not found." );

		var allProducts = await productService.GetAllProductsAsync();
		var allProductSuppliers = ( await supplierService.GetAllProductSuppliersAsync() )
			.Where( ps => ps.SupplierId == supplierId )
			.ToList();

		var productsByName = allProducts
			.Where( p => !string.IsNullOrWhiteSpace( p.ProductName ) )
			.GroupBy( p => p.ProductName!, StringComparer.OrdinalIgnoreCase )
			.ToDictionary( g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase );

		var existingCodes = new HashSet<string>(
			allProducts
				.Select( p => p.ProductCode )
				.Where( c => !string.IsNullOrWhiteSpace( c ) )!
				.Select( c => c! ),
			StringComparer.OrdinalIgnoreCase );

		var supplierRowsByProductId = allProductSuppliers
			.GroupBy( ps => ps.ProductId )
			.ToDictionary( g => g.Key, g => g.First() );

		var scrapedItems = ( await ScrapeItemsAsync( cancellationToken ) ).ToList();
		var runResult = new CornwallDowelImportRunResult();

		foreach ( var item in scrapedItems )
		{
			item.GeneratedProductCode = BuildProductCode( item.Name, existingCodes );

			if ( !productsByName.TryGetValue( item.Name, out var product ) )
			{
				byte[]? productImage = null;
				try
				{
					productImage = await DownloadImageAsync( item.AbsoluteImageUrl, cancellationToken );
				}
				catch
				{
					productImage = null;
				}

				var newProductId = await productService.InsertNewProductAsync( CreateProductParameters( item, productImage, defaults ) );
				product = new ProductModel
				{
					ProductId = newProductId,
					ProductName = item.Name,
					ProductCode = item.GeneratedProductCode,
					ProductPrice = item.Price,
					ProductDimensions = BuildDimensionsText( item.Name ),
					ProductImage = productImage
				};

				allProducts.Add( product );
				productsByName[item.Name] = product;
				existingCodes.Add( item.GeneratedProductCode );
				runResult.CreatedProducts++;
			}
			else
			{
				runResult.ReusedProducts++;
			}

			var supplierRow = supplierRowsByProductId.GetValueOrDefault( product.ProductId );
			var action = DetermineSupplierRowAction( supplierRow );

			var upsertModel = new ProductSupplierModel
			{
				ProductSupplierId = supplierRow?.ProductSupplierId ?? 0,
				ProductId = product.ProductId,
				SupplierId = supplier.Id,
				CurrencyId = supplier.CurrencyId,
				ProductNumber = item.ProductNumber,
				ProductName = item.Name,
				Price = item.Price,
				URL = item.AbsoluteProductUrl
			};

			var upsertedId = await supplierService.UpsertProductSupplierAsync( upsertModel );
			upsertModel.ProductSupplierId = upsertedId;
			supplierRowsByProductId[product.ProductId] = upsertModel;

			if ( action == SupplierRowAction.InsertNew )
			{
				runResult.CreatedSupplierRows++;
			}
			else
			{
				runResult.UpdatedSupplierRows++;
			}

			runResult.Items.Add( item );
		}

		return runResult;
	}

	private async Task<string> GetHtmlAsync( string url, CancellationToken cancellationToken )
	{
		using var response = await _httpClient.GetAsync( url, cancellationToken );
		response.EnsureSuccessStatusCode();
		return await response.Content.ReadAsStringAsync( cancellationToken );
	}

	private static List<string> ExtractSubPageUrls( string rootHtml )
	{
		var urls = new List<string>();
		foreach ( Match match in DowelLinkRegex.Matches( rootHtml ) )
		{
			var relativeUrl = System.Net.WebUtility.HtmlDecode( match.Groups[1].Value ).Trim();
			if ( !ExpectedSubPages.Any( p => relativeUrl.EndsWith( p, StringComparison.OrdinalIgnoreCase ) ) )
			{
				continue;
			}

			var absoluteUrl = relativeUrl.StartsWith( "http", StringComparison.OrdinalIgnoreCase )
				? relativeUrl
				: new Uri( new Uri( AcatalogBaseUrl ), relativeUrl ).ToString();

			if ( !urls.Contains( absoluteUrl, StringComparer.OrdinalIgnoreCase ) )
			{
				urls.Add( absoluteUrl );
			}
		}

		return urls;
	}

	private static List<CornwallDowelImportItem> ParseProductPage( string html )
	{
		var items = new List<CornwallDowelImportItem>();
		var seenProductUrls = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

		foreach ( Match anchorMatch in ProductAnchorRegex.Matches( html ) )
		{
			var headings = HeadingRegex.Matches( anchorMatch.Groups[2].Value )
				.Select( m => HtmlDecodeAndStrip( m.Groups[1].Value ) )
				.Where( s => !string.IsNullOrWhiteSpace( s ) )
				.ToList();

			if ( headings.Count < 2 )
			{
				continue;
			}

			var productName = headings[0];
			if ( !productName.Contains( "Dowel", StringComparison.OrdinalIgnoreCase )
				&& !productName.Contains( "Dowl", StringComparison.OrdinalIgnoreCase ) )
			{
				continue;
			}

			var productNumber = headings[1];
			var productUrl = System.Net.WebUtility.HtmlDecode( anchorMatch.Groups[1].Value ).Trim();
			if ( !seenProductUrls.Add( productUrl ) )
			{
				continue;
			}

			var afterAnchor = html.Substring(
				anchorMatch.Index,
				Math.Min( 1800, html.Length - anchorMatch.Index ) );

			var beforeAnchorStart = Math.Max( 0, anchorMatch.Index - 2500 );
			var beforeAnchor = html.Substring(
				beforeAnchorStart,
				anchorMatch.Index - beforeAnchorStart );

			var priceMatch = PriceRegex.Match( afterAnchor );
			var imageMatches = ImageRegex.Matches( beforeAnchor );

			if ( !priceMatch.Success )
			{
				continue;
			}

			var price = double.Parse(
				priceMatch.Groups[1].Value,
				CultureInfo.InvariantCulture );

			var absoluteProductUrl = productUrl.StartsWith( "http", StringComparison.OrdinalIgnoreCase )
				? productUrl
				: new Uri( new Uri( AcatalogBaseUrl ), productUrl ).ToString();

			var relativeImageUrl = imageMatches.Count > 0
				? System.Net.WebUtility.HtmlDecode( imageMatches[^1].Groups[1].Value ).Trim()
				: string.Empty;

			var absoluteImageUrl = string.IsNullOrWhiteSpace( relativeImageUrl )
				? string.Empty
				: relativeImageUrl.StartsWith( "http", StringComparison.OrdinalIgnoreCase )
					? relativeImageUrl
					: new Uri( new Uri( AcatalogBaseUrl ), relativeImageUrl ).ToString();

			items.Add( new CornwallDowelImportItem
			{
				Name = productName,
				ProductNumber = productNumber,
				Price = price,
				RelativeProductUrl = productUrl,
				AbsoluteProductUrl = absoluteProductUrl,
				AbsoluteImageUrl = absoluteImageUrl,
				MaterialCode = GetMaterialCode( productName )
			} );
		}

		return items;
	}

	private static string HtmlDecodeAndStrip( string value )
	{
		var decoded = System.Net.WebUtility.HtmlDecode( value );
		return Regex.Replace( decoded, "<.*?>", string.Empty ).Trim();
	}

	private static string GetMaterialCode( string productName )
	{
		if ( productName.Contains( "Balsa", StringComparison.OrdinalIgnoreCase ) )
		{
			return "BA";
		}

		if ( productName.Contains( "Birch", StringComparison.OrdinalIgnoreCase ) )
		{
			return "BI";
		}

		if ( productName.Contains( "Lime", StringComparison.OrdinalIgnoreCase ) )
		{
			return "LI";
		}

		if ( productName.Contains( "Sapelli", StringComparison.OrdinalIgnoreCase ) )
		{
			return "SA";
		}

		if ( productName.Contains( "Walnut", StringComparison.OrdinalIgnoreCase ) )
		{
			return "WA";
		}

		if ( productName.Contains( "Ramin", StringComparison.OrdinalIgnoreCase ) )
		{
			return "RA";
		}

		return "UN";
	}

	private static Dictionary<string, object?> CreateProductParameters( CornwallDowelImportItem item, byte[]? productImage, CornwallDowelImportDefaults defaults )
	{
		return new Dictionary<string, object?>
		{
			{ $"@{DBNames.ProductFieldNameBrandId}", defaults.BrandId },
			{ $"@{DBNames.ProductFieldNameCategoryId}", defaults.CategoryId },
			{ $"@{DBNames.ProductFieldNameCode}", item.GeneratedProductCode },
			{ $"@{DBNames.ProductFieldNameDimensions}", BuildDimensionsText( item.Name ) },
			{ $"@{DBNames.ProductFieldNameHide}", 0 },
			{ $"@{DBNames.ProductFieldNameImage}", productImage },
			{ $"@{DBNames.ProductFieldNameImageRotationAngle}", 0d },
			{ $"@{DBNames.ProductFieldNameMemo}", $"Imported from Cornwall Model Boats on {DateTime.UtcNow:yyyy-MM-dd}." },
			{ $"@{DBNames.ProductFieldNameMinimalStock}", 0d },
			{ $"@{DBNames.ProductFieldNameName}", item.Name },
			{ $"@{DBNames.ProductFieldNamePrice}", item.Price },
			{ $"@{DBNames.ProductFieldNameProjectCosts}", 0 },
			{ $"@{DBNames.ProductFieldNameStandardOrderQuantity}", 0d },
			{ $"@{DBNames.ProductFieldNameStorageId}", defaults.StorageId },
			{ $"@{DBNames.ProductFieldNameUnitId}", defaults.UnitId }
		};
	}

	private static (string Diameter, string Length) ParseDimensions( string productName )
	{
		var match = DimensionsRegex.Match( productName );
		if ( !match.Success )
		{
			return ("00", "0000");
		}

		var diameter = match.Groups[1].Value;
		var length = match.Groups[2].Value;
		return (diameter, length);
	}

	private static string BuildDimensionsText( string productName )
	{
		var (diameter, length) = ParseDimensions( productName );
		return diameter == "00" && length == "0000"
			? string.Empty
			: $"{diameter}mm x {length}mm";
	}

	private static HttpClient CreateHttpClient()
	{
		var client = new HttpClient();
		client.DefaultRequestHeaders.UserAgent.ParseAdd( "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/136.0 Safari/537.36" );
		return client;
	}
}
