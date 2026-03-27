namespace Modelbouwer.Models;

public class OrderLineModel
{
	public string? Received { get; set; }
	public string? ProductNumber { get; set; }
	public string? ProductDescription { get; set; }
	public decimal Price { get; set; }
	public decimal Amount { get; set; }
	public decimal RowTotal { get; set; }
}