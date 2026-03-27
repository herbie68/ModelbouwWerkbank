namespace Modelbouwer.Models;

public class OrderHeaderModel
{
	public int OrderId { get; set; }
	public string? OrderNumber { get; set; }
	public string? OrderDate { get; set; }
	public decimal ShippingCosts { get; set; }
	public decimal OrderCosts { get; set; }
	public decimal OrderTotal { get; set; }

	// TODO: Creade keys in the ResX library for the following text and use those keys here instead of hardcoding the text
	public string OrderSummary =>
	$"Besteldatum: {OrderDate}   " +
	$"Ordernummer: {OrderNumber}   " +
	$"Verzendkosten: {ShippingCosts:C2}   " +
	$"Orderkosten: {OrderCosts:C2}   " +
	$"Totaal: {OrderTotal:C2}";

	public ObservableCollection<OrderLineModel> OrderLines { get; set; } = [ ];
}
