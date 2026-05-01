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

	// Constructor
	public SupplierPageViewModel
		(
			ISupplierService dataService,
			ICountryService countryService,
			ICurrencyService currencyService,
			IContactService contactService,
			IContactTypeService contactTypeService,
			IEntityValidator<SupplierModel> validator
		) : base( validator )
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

	#region Collections & Selected Items
	public ObservableCollection<CountryModel> SupplierCountry { get; } = [ ];
	public ObservableCollection<CurrencyModel> SupplierCurrency { get; } = [ ];
	public ObservableCollection<ContactTypeModel> SupplierContactFunctions { get; } = [ ];
	public ObservableCollection<SupplierContactModel> Contacts { get; } = [ ];
	public ObservableCollection<SupplierContactModel> FilteredContacts { get; } = [ ];

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
	#endregion

	#region Load Methods
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
	#endregion

	#region SelectedSupplier + Filter
	private SupplierModel? _previousSupplier;
	//public SupplierModel? SelectedSupplier => SelectedItem;

	private void UpdateFilteredContacts()
	{
		FilteredContacts.Clear();

		if ( SelectedItem == null )
		{
			RaiseContactCounters();
			return;
		}

		foreach ( var c in Contacts.Where( c => c.SupplierId == SelectedItem.Id ) )
			FilteredContacts.Add( c );


		RaiseContactCounters();
	}

	private void RaiseContactCounters()
	{
		OnPropertyChanged( nameof( TotalContactCount ) );
	}

	public int TotalContactCount => FilteredContacts.Count;
	#endregion

	#region CRUD Contacts
	private IRelayCommand? _addContactCommand;
	public new IRelayCommand AddContactCommand => _addContactCommand ??= new RelayCommand( AddContact );

	private IRelayCommand? _deleteContactCommand;
	public new IRelayCommand DeleteContactCommand => _deleteContactCommand ??= new RelayCommand( DeleteContact, () => SelectedContact != null );

	private IRelayCommand? _saveContactCommand;
	public new IRelayCommand SaveContactCommand => _saveContactCommand ??= new RelayCommand( SaveContact );

	#region Relay command for going to the supplier website
	[RelayCommand( CanExecute = nameof( CanOpenWebsite ) )]
	private void OpenWebsite()
	{
		if ( string.IsNullOrWhiteSpace( SelectedItem?.Url ) )
			return;

		ProcessStartInfo startInfo = new()
		{
			FileName = SelectedItem.Url,
			UseShellExecute = true
		};

		Process.Start( startInfo );
	}

	private bool CanOpenWebsite()
	{
		return !string.IsNullOrWhiteSpace( SelectedItem?.Url );
	}
	#endregion


	private void AddContact()
	{
		if ( SelectedItem == null )
			return;

		var newContact = new SupplierContactModel
		{
			SupplierContactId = 0,
			SupplierId = SelectedItem.Id,
			Name = string.Empty,
			ContactTypeId = SupplierContactFunctions.FirstOrDefault()?.ContactTypeId ?? 0
		};
		Contacts.Add( newContact );
		FilteredContacts.Add( newContact );

		SelectedContact = newContact;

		RaiseContactCounters();
	}

	private void DeleteContact()
	{
		if ( SelectedContact == null )
			return;

		Contacts.Remove( SelectedContact );
		FilteredContacts.Remove( SelectedContact );

		RaiseContactCounters();
	}

	private void SaveContact()
	{
		// Implement contact save logic here
		// This would typically call a service method to persist the contact
	}
	#endregion


	// Commands
	public IRelayCommand AddSupplierCommand => AddCommand;
	public IAsyncRelayCommand SaveSupplierCommand => SaveCommand;
	public IRelayCommand DeleteSupplierCommand => DeleteCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );


	private IRelayCommand? _clearSearchCommand;


	// Override SelectedItem changed om DefaultSupplier te zetten
	protected override void OnSelectedItemChanged( SupplierModel? oldValue, SupplierModel? newValue )
	{
		base.OnSelectedItemChanged( oldValue, newValue );

		SelectedCurrency = SupplierCurrency.FirstOrDefault( c => c.CurrencyId == newValue?.CurrencyId );

		SelectedCountry = SupplierCountry.FirstOrDefault( c => c.CountryId == newValue?.CountryId );

		UpdateFilteredContacts();

		OpenWebsiteCommand.NotifyCanExecuteChanged();

		_previousSupplier = newValue;
	}


	// Properties voor UI binding
	public ObservableCollection<SupplierModel> Suppliers => Items;
	public int TotalSupplierCount => TotalItemCount;
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


}
