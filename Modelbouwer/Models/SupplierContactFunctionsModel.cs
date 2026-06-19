namespace Modelbouwer.Models;

public partial class SupplierContactFunctionsModel : ObservableObject
{
	[ObservableProperty] public int _contactTypeId;
	[ObservableProperty] public string ? _contactTypeName;

	public static readonly Dictionary<string, string> HeaderToPropertyMap = new()
	{
		{ DBNames.ContactTypeFieldNameId, "ContactTypeId" },
		{ DBNames.ContactTypeFieldNameName, "ContactTypeName" }
	};
}