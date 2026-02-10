namespace Modelbouwer.Models;

public class SupplierContactModel : ObservableObject
{
	public int SupplierContactId { get; set; }
	public int SupplierId { get; set; }
	public int ContactId { get; set; }
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
	public string? Name { get; set; }
	public string? Mail { get; set; }
	public string? Phone { get; set; }

	// Lookup voor contacttypes
	public IReadOnlyList<ContactTypeModel>? ContactTypeList { get; set; }

	public string? ContactTypeName => ContactTypeList?.FirstOrDefault( ct => ct.ContactTypeId == ContactTypeId )?.ContactTypeName;

	public void RefreshContactTypeName() => OnPropertyChanged( nameof( ContactTypeName ) );
}
