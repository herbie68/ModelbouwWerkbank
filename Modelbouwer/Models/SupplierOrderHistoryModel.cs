namespace Modelbouwer.Models;

public class SupplierOrderHistoryModel : ObservableObject
{
	private int OrderId { get; set; }
	private int ProductId { get; set; }
	private int SupplierId { get; set; }
	private double Amount { get; set; }
	private double CurrencyConversionRate { get; set; }
	private double OrderCosts { get; set; }
	private double OrderTotal { get; set; }
	private double Price { get; set; }
	private double RowTotal { get; set; }
	private double ShippingCosts { get; set; }
	private string? OrderNumber { get; set; }
	private string? ProductDescription { get; set; }
	private string? ProductNumber { get; set; }
	private string? Received { get; set; }
	private DateOnly? OrderDate { get; set; }
}
