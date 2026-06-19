namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public class StockOrderProductDialogViewModelTests
{
	[TestMethod]
	public void CreateFromProductAndEmptyProductSupplier_FallsBackToProductValues()
	{
		var product = new ProductModel
		{
			ProductId = 5,
			ProductCode = "P-005",
			ProductName = "Wheel Set",
			ProductPrice = 12.5,
			ProductStandardQuantity = 3
		};

		var supplier = new SupplierModel
		{
			Id = 11,
			CurrencyId = 2
		};

		var model = StockOrderProductDialogModel.Create( product, supplier, null );

		Assert.AreEqual( 5, model.ProductId );
		Assert.AreEqual( 11, model.SupplierId );
		Assert.AreEqual( "P-005", model.SupplierProductNumber );
		Assert.AreEqual( "Wheel Set", model.SupplierProductName );
		Assert.AreEqual( 12.5, model.UnitPrice );
		Assert.AreEqual( 3, model.Amount );
	}

	[TestMethod]
	public void ConfirmCommand_WithPositiveAmountAndPrice_CompletesSuccessfully()
	{
		var model = new StockOrderProductDialogModel
		{
			ProductId = 5,
			SupplierId = 11,
			SupplierProductName = "Wheel Set",
			SupplierProductNumber = "P-005",
			UnitPrice = 12.5,
			Amount = 2
		};

		var vm = new StockOrderProductDialogViewModel( model );

		var confirmed = vm.TryConfirm( out var errorMessage );

		Assert.IsTrue( confirmed );
		Assert.IsNull( errorMessage );
		Assert.AreEqual( 25.0, vm.Model.RowTotal );
	}

	[TestMethod]
	public void RowTotal_RaisesPropertyChanged_WhenAmountOrPriceChanges()
	{
		var model = new StockOrderProductDialogModel
		{
			UnitPrice = 10,
			Amount = 2
		};

		List<string?> changedProperties = [];
		model.PropertyChanged += ( _, args ) => changedProperties.Add( args.PropertyName );

		model.UnitPrice = 12.5;
		model.Amount = 3;

		Assert.IsTrue( changedProperties.Contains( nameof( StockOrderProductDialogModel.RowTotal ) ) );
		Assert.AreEqual( 37.5, model.RowTotal );
	}
}