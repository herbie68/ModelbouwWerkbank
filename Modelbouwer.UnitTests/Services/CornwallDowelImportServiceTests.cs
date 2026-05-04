namespace Modelbouwer.UnitTests.Services;

[TestClass]
public class CornwallDowelImportServiceTests
{
	[TestMethod]
	public async Task ScrapeItemsAsync_WithRealisticCardMarkup_ReturnsParsedItem()
	{
		const string rootHtml = """
			<a href="Balsa_Dowel.html">Balsa Dowel</a>
			""";

		const string subPageHtml = """
			<div class="marketing-list-entry"><div class="thumbnail"><div class="image"><a href="Amati-Lime-Dowel-1mm-x-1-Metre-A2525_01.html#SID=463"><picture><img src="b252501.jpg" border="0" title="Lime Dowel 1mm x 1000mm" alt="Lime Dowel 1mm x 1000mm" /></picture></a></div><div class="caption"><a class="product-name" href="Amati-Lime-Dowel-1mm-x-1-Metre-A2525_01.html#SID=463"><h2>Lime Dowel 1mm x 1000mm</h2><h2>A2525/01</h2></a><div class="col-12"><div class="online-price-sec"><Actinic:PRICES PROD_REF="A2525/01" RETAIL_PRICE_PROMPT="Price:" class="w-100"><div id="idA2525/01StaticPrice" class="static-price"><span class="product-price"><span class="price"><span class="price-num">&nbsp;£0.91 </span> <span class="each">&nbsp;each</span></span></span></div></Actinic:PRICES></div></div></div><fieldset class="form-group quantity-box w-50 w-md-100"></fieldset></div>
			""";

		using var httpClient = new HttpClient( new StubHttpMessageHandler( request =>
		{
			var html = request.RequestUri!.AbsoluteUri.EndsWith( "Balsa_Dowel.html", StringComparison.OrdinalIgnoreCase )
				? subPageHtml
				: rootHtml;

			return new HttpResponseMessage( System.Net.HttpStatusCode.OK )
			{
				Content = new StringContent( html )
			};
		} ) );

		var service = new CornwallDowelImportService( httpClient );

		var result = await service.ScrapeItemsAsync();

		Assert.AreEqual( 1, result.Count );
		Assert.AreEqual( "Lime Dowel 1mm x 1000mm", result[0].Name );
		Assert.AreEqual( "A2525/01", result[0].ProductNumber );
		Assert.AreEqual( 0.91d, result[0].Price, 0.0001d );
		Assert.AreEqual( "https://www.cornwallmodelboats.co.uk/acatalog/b252501.jpg", result[0].AbsoluteImageUrl );
		Assert.AreEqual( "https://www.cornwallmodelboats.co.uk/acatalog/Amati-Lime-Dowel-1mm-x-1-Metre-A2525_01.html#SID=463", result[0].AbsoluteProductUrl );
	}

	[TestMethod]
	public void BuildProductCode_LimeDowel10x1000_ReturnsExpectedShortCode()
	{
		var existingCodes = new HashSet<string>( StringComparer.OrdinalIgnoreCase );

		var result = CornwallDowelImportService.BuildProductCode(
			"Lime Dowel 10mm x 1000mm",
			existingCodes );

		Assert.AreEqual( "CWD-LI-10X1000", result );
	}

	[TestMethod]
	public void BuildProductCode_WhenCodeAlreadyExists_AppendsNumericSuffix()
	{
		var existingCodes = new HashSet<string>( StringComparer.OrdinalIgnoreCase )
		{
			"CWD-WA-14X1000"
		};

		var result = CornwallDowelImportService.BuildProductCode(
			"Walnut Dowl 14mm x 1000mm",
			existingCodes );

		Assert.AreEqual( "CWD-WA-14X1000-2", result );
	}

	[TestMethod]
	public void DetermineSupplierRowAction_WhenExistingRowPresent_ReturnsUpdate()
	{
		var existingRow = new Modelbouwer.Model.ProductSupplierModel
		{
			ProductSupplierId = 42,
			ProductId = 10,
			SupplierId = 1
		};

		var result = CornwallDowelImportService.DetermineSupplierRowAction( existingRow );

		Assert.AreEqual( SupplierRowAction.UpdateExisting, result );
	}

	[TestMethod]
	public void DetermineSupplierRowAction_WhenNoExistingRowPresent_ReturnsInsert()
	{
		var result = CornwallDowelImportService.DetermineSupplierRowAction( null );

		Assert.AreEqual( SupplierRowAction.InsertNew, result );
	}

	private sealed class StubHttpMessageHandler : HttpMessageHandler
	{
		private readonly Func<HttpRequestMessage, HttpResponseMessage> _responseFactory;

		public StubHttpMessageHandler( Func<HttpRequestMessage, HttpResponseMessage> responseFactory )
		{
			_responseFactory = responseFactory;
		}

		protected override Task<HttpResponseMessage> SendAsync( HttpRequestMessage request, CancellationToken cancellationToken )
		{
			return Task.FromResult( _responseFactory( request ) );
		}
	}
}
