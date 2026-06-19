namespace Modelbouwer.Models;

public partial class SupplierContactModel : ObservableObject
{
	[ObservableProperty] public int _supplierContactId;
	[ObservableProperty] public int _supplierId;
	[ObservableProperty] public int _contactId;

	private int _contactTypeId;
	public int ContactTypeId
	{
		get => _contactTypeId;
		set
		{
			if ( SetProperty( ref _contactTypeId, value ) )
			{
				// Notify ContactTypeName changes whenever ContactTypeId changes
				OnPropertyChanged( nameof( ContactTypeName ) );
			}
		}
	}
	[ObservableProperty] public string? _name;
	[ObservableProperty] public string? _mail;
	[ObservableProperty] public string? _phone;

	// Lookup voor contacttypes
	[ObservableProperty] public IReadOnlyList<ContactTypeModel>? _contactTypeList;

	public string? ContactTypeName => ContactTypeList?.FirstOrDefault( ct => ct.ContactTypeId == ContactTypeId )?.ContactTypeName;

	public void RefreshContactTypeName() => OnPropertyChanged( nameof( ContactTypeName ) );
}