using DocumentFormat.OpenXml.Office2010.Excel;

namespace Modelbouwer.Models;

public partial class ProductModel : ObservableObject
{
	[ObservableProperty]
	private byte [ ]? _productImage;

	[ObservableProperty]
	private double _productMinimalStock;

	[ObservableProperty]
	private double _productPrice;

	[ObservableProperty]
	private double _productPackagePrice;

	[ObservableProperty]
	private double _productStandardQuantity;

	[ObservableProperty]
	private int _productBrandId;

	[ObservableProperty]
	private int _productCategoryId;

	[ObservableProperty]
	private int _productId;

	[ObservableProperty]
	private int _productProjectCosts;

	[ObservableProperty]
	private int _productStorageId;

	[ObservableProperty]
	private int _productUnitId;

	[ObservableProperty]
	private int _productHide;

	[ObservableProperty]
	private string? _productCode;

	[ObservableProperty]
	private string? _productDimensions;

	[ObservableProperty]
	private string? _productImageRotationAngle;

	[ObservableProperty]
	private string? _productMemo;

	[ObservableProperty]
	private string? _productName;

	// Define the property that you want to use in TLists (for example in the errorList
	public string Name => _productName;

	/// <summary>
	/// Updates the package price when the unit price changes, keeping package and unit pricing consistent.
	/// </summary>
	/// <param name="value">The new unit price; if <see cref="ProductStandardQuantity"/> is greater than zero, the package price is set to this value multiplied by the standard quantity (rounded to 2 decimal places).</param>
	partial void OnProductPriceChanged( double value )
	{
		if ( ProductStandardQuantity > 0 )
		{
			ProductPackagePrice = Math.Round( value * ProductStandardQuantity, 2 );
		}
	}

	/// <summary>
	/// If ProductStandardQuantity is greater than zero, sets ProductPrice to the package price divided by ProductStandardQuantity, rounded to 6 decimal places.
	/// </summary>
	/// <param name="value">The new package price.</param>
	partial void OnProductPackagePriceChanged( double value )
	{
		if ( ProductStandardQuantity > 0 )
		{
			ProductPrice = Math.Round( value / ProductStandardQuantity, 6 );
		}
	}

	/// <summary>
	/// Updates the related price fields to remain consistent when the product's standard quantity changes.
	/// </summary>
	/// <param name="value">The new standard quantity used to recalculate dependent prices. If <see cref="ProductPrice"/> is greater than zero, <see cref="ProductPackagePrice"/> is set to ProductPrice * value rounded to 2 decimals; otherwise, if <see cref="ProductPackagePrice"/> is greater than zero, <see cref="ProductPrice"/> is set to ProductPackagePrice / value rounded to 6 decimals.</param>
	partial void OnProductStandardQuantityChanged( double value )
	{
		// Herbereken indien nodig
		if ( ProductPrice > 0 )
		{
			ProductPackagePrice = Math.Round( ProductPrice * value, 2 );
		}
		else if ( ProductPackagePrice > 0 )
		{
			ProductPrice = Math.Round( ProductPackagePrice / value, 6 );
		}
	}

	#region ColumnMappings
	public static readonly Dictionary<string, string[]> ColumnMappings = new()
	{
		[nameof(Id)] = [ "ID" ],

		[nameof(ProductCode)] =
		[
			"Zoek naam",
			"Search name",
			"Suchname" ],

		[nameof(ProductName)] = [
			"Produktnaam",
			"Product name",
			"Produktname" ],

	};
	#endregion

	// Mapping dictionary for mapping Database Header to Property name
	public static readonly Dictionary<string, string> HeaderToPropertyMap = new()
	{
		{ DBNames.ProductFieldNameBrandId, "_productBrandId" },
		{ DBNames.ProductFieldNameCategoryId, "_productCategoryId" },
		{ DBNames.ProductFieldNameCode, "_productCode" },
		{ DBNames.ProductFieldNameDimensions, "_productDimensions" },
		{ DBNames.ProductFieldNameId, "_productId" },
		{ DBNames.ProductFieldNameImage, "_productImage" },
		{ DBNames.ProductFieldNameImageRotationAngle, "_productImageRotationAngle" },
		{ DBNames.ProductFieldNameMinimalStock, "_productMinimalStock" },
		{ DBNames.ProductFieldNameName, "_productName" },
		{ DBNames.ProductFieldNamePrice, "_productPrice" },
		{ DBNames.ProductFieldNameProjectCosts, "_productProjectCosts" },
		{ DBNames.ProductFieldNameStandardOrderQuantity, "_productStandardQuantity" },
		{ DBNames.ProductFieldNameStorageId, "_productStorageId" },
		{ DBNames.ProductFieldNameUnitId, "_productUnitId" },
	};

	/// <summary>
/// Initializes a new instance of ProductModel with default property values.
/// </summary>
public ProductModel() { }

	/// <summary>
	/// Initializes a new ProductModel by copying price-related values from an existing instance.
	/// </summary>
	/// <param name="other">The source ProductModel whose price, package price, and standard quantity will be copied.</param>
	public ProductModel( ProductModel other )
	{
		_productPrice = other._productPrice;
		_productPackagePrice = other._productPackagePrice;
		_productStandardQuantity = other._productStandardQuantity;
	}

}