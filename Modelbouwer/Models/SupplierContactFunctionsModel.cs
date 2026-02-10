namespace Modelbouwer.Models;

public class SupplierContactFunctionsModel : ObservableObject
{
	public int ContactTypeId { get; set; }
	public string? ContactTypeName { get; set; }

	public static readonly Dictionary<string, string> HeaderToPropertyMap = new()
	{
		{ DBNames.ContactTypeFieldNameId, "ContactTypeId" },
		{ DBNames.ContactTypeFieldNameName, "ContactTypeName" }
	};
}
