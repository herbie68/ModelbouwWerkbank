namespace Modelbouwer.Models;

public class SupplierContactModel : ObservableObject
{
	public int SupplierContactId { get; set; }
	public int SupplierId { get; set; }
	public int ContactTypeId { get; set; }
	public string? Name { get; set; }
	public string? Mail { get; set; }
	public string? Phone { get; set; }
}
