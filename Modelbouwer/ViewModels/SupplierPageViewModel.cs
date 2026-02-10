using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public partial class SupplierPageViewModel : EntityPageViewModel<SupplierModel>
{
	private readonly ISupplierService _dataService;
	private readonly ICountryService _countryService;
	private readonly ICurrencyService _currencyService;
	private readonly IContactService _contactService;
	private readonly IContactTypeService _contactTypeService;

	private int? _lastSelectedSupplierId;

	public SupplierModel? SelectedSupplier
	{
		get => SelectedItem;
		set
		{
			SelectedItem = value;
			UpdateFilteredContacts();
		}
	}

	// Commands
	public IRelayCommand AddSupplierCommand => AddCommand;
	public IAsyncRelayCommand SaveSupplierCommand => SaveCommand;
	public IRelayCommand DeleteSupplierCommand => DeleteCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );

	private SupplierModel? _previousSupplier;

	private IRelayCommand? _clearSearchCommand;

	// Collections for dropdowns
	public ObservableCollection<CountryModel> SupplierCountry { get; } = [ ];
	public ObservableCollection<CurrencyModel> SupplierCurrency { get; } = [ ];
	public ObservableCollection<ContactTypeModel> SupplierContactFunctions { get; } = [ ];
	public ObservableCollection<SupplierContactModel> Contacts { get; } = [ ];

	// Selected items for dropdowns
	private CountryModel? _selectedCountry;
	public CountryModel? SelectedCountry
	{
		get => _selectedCountry;
		set
		{
			if ( SetProperty( ref _selectedCountry, value ) && SelectedItem != null && value != null )
			{
				SelectedItem.CountryId = value.CountryId;
			}
		}
	}

	private CurrencyModel? _selectedCurrency;
	public CurrencyModel? SelectedCurrency
	{
		get => _selectedCurrency;
		set
		{
			if ( SetProperty( ref _selectedCurrency, value ) && SelectedItem != null && value != null )
			{
				SelectedItem.CurrencyId = value.CurrencyId;
			}
		}
	}

	private SupplierContactModel? _selectedContact;
	public SupplierContactModel? SelectedContact
	{
		get => _selectedContact;
		set
		{
			if ( SetProperty( ref _selectedContact, value ) )
			{
				if ( value != null )
				{
					// Zet de combobox SelectedContactFunction automatisch
					SelectedContactFunction = SupplierContactFunctions.FirstOrDefault( ct => ct.ContactTypeId == value.ContactTypeId );
				}
			}
		}
	}

	private ContactTypeModel? _selectedContactFunction;
	public ContactTypeModel? SelectedContactFunction
	{
		get => _selectedContactFunction;
		set
		{
			if ( SetProperty( ref _selectedContactFunction, value ) && SelectedContact != null && value != null )
			{
				SelectedContact.ContactTypeId = value.ContactTypeId;
			}
		}
	}

	#region Filter contacts on selected supplier
	public ObservableCollection<SupplierContactModel> FilteredContacts { get; } = new();

	private void UpdateFilteredContacts()
	{
		FilteredContacts.Clear();

		if ( SelectedSupplier == null )
			return;

		foreach ( var contact in Contacts.Where( c => c.SupplierId == SelectedSupplier.Id ) )
			FilteredContacts.Add( contact );
	}
	#endregion


	// Contact commands
	private IRelayCommand? _addContactCommand;
	public IRelayCommand AddContactCommand => _addContactCommand ??= new RelayCommand( AddContact );

	private IRelayCommand? _deleteContactCommand;
	public IRelayCommand DeleteContactCommand => _deleteContactCommand ??= new RelayCommand( DeleteContact, () => SelectedContact != null );

	private IRelayCommand? _saveContactCommand;
	public IRelayCommand SaveContactCommand => _saveContactCommand ??= new RelayCommand( SaveContact );

	// Import status properties
	private string _importStatus = string.Empty;
	public string ImportStatus
	{
		get => _importStatus;
		set => SetProperty( ref _importStatus, value );
	}

	private bool _isImporting;
	public bool IsImporting
	{
		get => _isImporting;
		set => SetProperty( ref _isImporting, value );
	}

	public int CountryId { get; set; }

	// Constructor
	public SupplierPageViewModel( ISupplierService dataService, ICountryService countryService, ICurrencyService currencyService, IContactService contactService,
	IContactTypeService contactTypeService, IEntityValidator<SupplierModel> validator ) : base( validator )
	{
		_dataService = dataService;

		_countryService = countryService;
		_currencyService = currencyService;
		_contactService = contactService ?? throw new ArgumentNullException( nameof( contactService ) );
		_contactTypeService = contactTypeService ?? throw new ArgumentNullException( nameof( contactTypeService ) );

		_ = LoadComboBoxesContentAsync();
		_ = LoadContactsAndFunctionsAsync();

		_ = ReloadCommand.ExecuteAsync( null );
	}

	private async Task LoadComboBoxesContentAsync()
	{
		SupplierCountry.Clear();
		SupplierCurrency.Clear();

		var countries = await _countryService.GetAllCountriesAsync();
		foreach ( var country in countries )
		{
			SupplierCountry.Add( country );
		}

		var currencies = await _currencyService.GetAllCurrenciesAsync();
		foreach ( var currency in currencies )
		{
			SupplierCurrency.Add( currency );
		}
	}

	private async Task LoadContactsAndFunctionsAsync()
	{
		// Load contact types
		var types = await _contactTypeService.GetAllContactTypesAsync();
		SupplierContactFunctions.Clear();
		foreach ( var t in types )
			SupplierContactFunctions.Add( t );

		// Load all contacts
		var allContacts = await _contactService.GetAllContactsAsync();
		Contacts.Clear();
		foreach ( var c in allContacts )
			Contacts.Add( c );

		// Koppel ContactType lookup
		foreach ( var contact in Contacts )
		{
			contact.ContactTypeList = SupplierContactFunctions;
			contact.RefreshContactTypeName();
		}

		// Update filtered view
		UpdateFilteredContacts();
	}

	// Override SelectedItem changed om DefaultSupplier te zetten
	protected override void OnSelectedItemChanged( SupplierModel? value )
	{
		base.OnSelectedItemChanged( value );

		SelectedCurrency = SupplierCurrency.FirstOrDefault( c => c.CurrencyId == value?.CurrencyId );

		SelectedCountry = SupplierCountry.FirstOrDefault( c => c.CountryId == value?.CountryId );

		_previousSupplier = value;
	}


	// Properties voor UI binding
	public ObservableCollection<SupplierModel> Suppliers => Items;
	public int TotalSupplierCount => TotalItemCount;
	public int TotalContactCount => TotalContactCount;
	public int VisibleSupplierCount
	{
		get => base.VisibleItemCount;
		set => base.VisibleItemCount = value;
	}

	// Filtering
	public bool FilterSupplier( object obj )
	{
		if ( obj is not SupplierModel Supplier )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		return Supplier.Name?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	// Abstract overrides voor CRUD
	protected override Task<List<SupplierModel>> LoadItemsAsync() => _dataService.GetAllSuppliersAsync();
	protected override Task<int> InsertAsync( SupplierModel item ) => _dataService.InsertNewSupplierAsync( CreateParameters( item ) );
	protected override Task UpdateAsync( SupplierModel item )
	{
		if ( SelectedItem == null )
			return Task.CompletedTask;

		_lastSelectedSupplierId = SelectedItem.Id;

		return _dataService.UpdateSupplierAsync( CreateParameters( SelectedItem ) );
	}
	protected override async Task DeleteAsync( SupplierModel item )
	{
		if ( item == null )
			return;

		var result = MessageBox.Show(
			$"{Lang.toolbarButtonActionDeleteMessageQuestionPrefix} '{item.Name}' {Lang.toolbarButtonActionDeleteMessageQuestionSuffix}",
			$"{Lang.toolbarButtonActionDeleteMessageButtonText}",
			MessageBoxButton.YesNo,
			MessageBoxImage.Warning
		);

		if ( result != MessageBoxResult.Yes )
			return;
		try
		{
			await _dataService.DeleteSupplierAsync( item.Id );
		}
		catch ( EntityInUseException ex )
		{
			MessageBox.Show(
				ex.Message,
				Lang.generalMessageboxWarningTitle,
				MessageBoxButton.OK,
				MessageBoxImage.Information
			);
		}
	}

	protected override int GetId( SupplierModel item ) => item.Id;
	protected override void SetId( SupplierModel item, int id ) => item.Id = id;

	protected override SupplierModel CreateNewItem() => new()
	{
		Id = 0,
		Name = string.Empty
	};

	protected override void OnItemsLoaded()
	{
		base.OnItemsLoaded();

		foreach ( var supplier in Suppliers )
		{
			supplier.CountryList = SupplierCountry;
		}

		OnPropertyChanged( nameof( TotalSupplierCount ) );
		OnPropertyChanged( nameof( TotalContactCount ) );

		if ( _lastSelectedSupplierId.HasValue )
		{
			var match = Suppliers.FirstOrDefault( p => p.Id == _lastSelectedSupplierId.Value );

			if ( match != null )
			{
				SelectedItem = match;
				return;
			}

			_lastSelectedSupplierId = null;
		}

		// Default Supplier selection (Highest Id)
		SelectSupplierWithHighestId();
	}

	private void SelectSupplierWithHighestId()
	{
		if ( Suppliers.Count == 0 )
		{
			SelectedItem = null;
			return;
		}

		SelectedItem = Suppliers
			.OrderByDescending( p => p.Id )
			.First();
	}

	// Parameter dictionary voor save
	private static Dictionary<string, object?> CreateParameters( SupplierModel c ) => new()
	{
		{ $"@{DBNames.SupplierFieldNameId}", c.Id },
		{ $"@{DBNames.SupplierFieldNameCode}", c.Code },
		{ $"@{DBNames.SupplierFieldNameName}", c.Name?.Trim() },
		{ $"@{DBNames.SupplierFieldNameMemo}", c.Memo }
	};

	// Contact management methods
	private void AddContact()
	{
		var newContact = new SupplierContactModel
		{
			SupplierContactId = 0,
			Name = string.Empty
		};
		Contacts.Add( newContact );
		SelectedContact = newContact;
	}

	private void DeleteContact()
	{
		if ( SelectedContact == null )
			return;

		Contacts.Remove( SelectedContact );
	}

	private void SaveContact()
	{
		// Implement contact save logic here
		// This would typically call a service method to persist the contact
	}

}
