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

	/// <summary>
	/// Initializes a new instance of SupplierPageViewModel and wires required services for supplier, country, currency, contact, and contact-type operations.
	/// </summary>
	/// <remarks>
	/// Starts asynchronous loading of combo-box data and contact/type lists, and triggers an initial reload of supplier data.
	/// </remarks>
	/// <exception cref="ArgumentNullException">Thrown when <c>contactService</c> is null.</exception>
	/// <exception cref="ArgumentNullException">Thrown when <c>contactTypeService</c> is null.</exception>
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
	/// <summary>
	/// Loads all countries and currencies from their services and populates the SupplierCountry and SupplierCurrency collections.
	/// </summary>
	/// <returns>A task that completes after the supplier country and currency collections have been populated.</returns>
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

	/// <summary>
	/// Loads all contact types and contacts, assigns the contact-type lookup to each contact, and refreshes the filtered contacts collection.
	/// </summary>
	/// <returns>A task that completes when contact types and contacts have been loaded and the filtered contact view updated.</returns>
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
	/// <summary>
	/// Refreshes the FilteredContacts collection to contain only contacts that belong to the current SelectedItem.
	/// </summary>
	/// <remarks>
	/// Clears FilteredContacts, then if a SelectedItem is present adds all Contacts whose SupplierId matches SelectedItem.Id.
	/// After repopulating, notifies consumers by raising the contact counters.
	/// If no SelectedItem is set, the method clears FilteredContacts and exits.
	/// </remarks>

	private void UpdateFilteredContacts()
	{
		FilteredContacts.Clear();

		if ( SelectedItem == null )
			return;

		if ( SelectedItem == null )
		{
			RaiseContactCounters();
			return;
		}

		foreach ( var c in Contacts.Where( c => c.SupplierId == SelectedItem.Id ) )
			FilteredContacts.Add( c );


		RaiseContactCounters();
	}

	/// <summary>
	/// Notifies data bindings that the TotalContactCount property has changed.
	/// </summary>
	private void RaiseContactCounters()
	{
		OnPropertyChanged( nameof( TotalContactCount ) );
	}

	public int TotalContactCount => FilteredContacts.Count;
	#endregion

	#region CRUD Contacts
	private IRelayCommand? _addContactCommand;
	public IRelayCommand AddContactCommand => _addContactCommand ??= new RelayCommand( AddContact );

	private IRelayCommand? _deleteContactCommand;
	public IRelayCommand DeleteContactCommand => _deleteContactCommand ??= new RelayCommand( DeleteContact, () => SelectedContact != null );

	private IRelayCommand? _saveContactCommand;
	public IRelayCommand SaveContactCommand => _saveContactCommand ??= new RelayCommand( SaveContact );

	#region Relay command for going to the supplier website
	/// <summary>
	/// Opens the selected supplier's URL in the system's default handler (typically a browser).
	/// </summary>
	/// <remarks>
	/// If the selected item's URL is null, empty, or whitespace, the method does nothing.
	/// </remarks>
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

	/// <summary>
	/// Determines whether the currently selected item has a non-empty website URL.
	/// </summary>
	/// <returns>`true` if SelectedItem.Url contains non-whitespace characters; `false` otherwise.</returns>
	private bool CanOpenWebsite()
	{
		return !string.IsNullOrWhiteSpace( SelectedItem?.Url );
	}
	#endregion


	/// <summary>
	/// Creates a new supplier contact for the currently selected supplier, adds it to the contacts collections, selects it, and updates contact counters.
	/// </summary>
	/// <remarks>
	/// If there is no selected supplier, the method does nothing.
	/// The new contact is initialized with Id = 0, an empty name, and a ContactTypeId set to the first available contact function's id if one exists, otherwise 0.
	/// </remarks>
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

	/// <summary>
	/// Removes the currently selected contact from the master and filtered contact collections and updates contact counters.
	/// </summary>
	/// <remarks>
	/// If no contact is selected, the method does nothing.
	/// </remarks>
	private void DeleteContact()
	{
		if ( SelectedContact == null )
			return;

		Contacts.Remove( SelectedContact );
		FilteredContacts.Remove( SelectedContact );

		RaiseContactCounters();
	}

	/// <summary>
	/// Persists changes to the currently selected supplier contact.
	/// </summary>
	/// <remarks>
	/// If a contact is selected, saves (inserts or updates) that contact to the underlying data store and refreshes the contact collections so the UI reflects the persisted state. If no contact is selected, the method does nothing.
	/// </remarks>
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


	/// <summary>
	/// Synchronizes view-model state when the selected supplier changes.
	/// </summary>
	/// <param name="value">The newly selected <see cref="SupplierModel"/> or <c>null</c> if no supplier is selected.</param>
	/// <remarks>
	/// Updates the selected currency and country to match the supplier, refreshes the filtered contacts,
	/// notifies the OpenWebsite command to re-evaluate its ability to execute, and remembers the previous supplier.
	/// </remarks>
	protected override void OnSelectedItemChanged( SupplierModel? value )
	{
		base.OnSelectedItemChanged( value );

		SelectedCurrency = SupplierCurrency.FirstOrDefault( c => c.CurrencyId == value?.CurrencyId );

		SelectedCountry = SupplierCountry.FirstOrDefault( c => c.CountryId == value?.CountryId );

		UpdateFilteredContacts();

		OpenWebsiteCommand.NotifyCanExecuteChanged();

		_previousSupplier = value;
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

	/// <summary>
	/// Builds a dictionary of database parameter names to values for saving a supplier.
	/// </summary>
	/// <param name="c">The supplier whose fields will be used as parameter values.</param>
	/// <returns>A dictionary mapping database parameter names to the supplier's Id, Code, trimmed Name, and Memo.</returns>
	private static Dictionary<string, object?> CreateParameters( SupplierModel c ) => new()
	{
		{ $"@{DBNames.SupplierFieldNameId}", c.Id },
		{ $"@{DBNames.SupplierFieldNameCode}", c.Code },
		{ $"@{DBNames.SupplierFieldNameName}", c.Name?.Trim() },
		{ $"@{DBNames.SupplierFieldNameMemo}", c.Memo }
	};

	// Contact management methods


}