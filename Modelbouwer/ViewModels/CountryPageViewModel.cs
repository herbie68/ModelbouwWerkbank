using System.Collections.ObjectModel;
using System.Diagnostics.Metrics;
using System.Windows;
using System.Windows.Controls;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Win32;

using Modelbouwer.Interfaces;
using Modelbouwer.Models;
using Modelbouwer.Services;

namespace Modelbouwer.ViewModels;

public partial class CountryPageViewModel : ObservableObject
{
	private readonly IEntityValidator<CountryModel> _countryValidator;
	private readonly ICountryService _countryService;

	private readonly CurrencyService _currencyService;

	public ObservableCollection<CountryModel> Countries { get; } = [];
	
	[ObservableProperty]
	private ObservableCollection<CurrencyModel> _currencies = [];
	
	[ObservableProperty]
	private CountryModel? _selectedCountry;

	[ObservableProperty]
	private string _searchText = string.Empty;

	[ObservableProperty]
	private int _visibleCountryCount;

	[ObservableProperty]
	private int _totalCountryCount;


	[ObservableProperty]
	private bool _isSaving;

	[ObservableProperty]
	private bool _isLoading;

	[ObservableProperty]
	private bool _isImporting;

	[ObservableProperty]
	private string _importStatus = string.Empty;

	//public int TotalCountryCount => Countries.Count;

	#region Country deletion check
	private bool _countryUsed;

	public bool CountryUsed
	{
		get => _countryUsed;
		set
		{
			if ( SetProperty( ref _countryUsed, value ) )
			{
				OnPropertyChanged( nameof( CanDeleteCountry ) );
				OnPropertyChanged( nameof( DeleteToolTipKey ) );
			}
		}
	}

	public string DeleteToolTipKey => CountryUsed
		? nameof( Language.toolbarButtonActionCanNotDelete )
		: nameof( Language.toolbarButtonActionDelete );

	public bool CanDeleteCountry => !CountryUsed;
	#endregion

	public CountryPageViewModel( ICountryService countryService, CurrencyService currencyService, IEntityValidator<CountryModel> countryValidator )
	{
		_countryService = countryService;
		_currencyService = currencyService;
		_countryValidator = countryValidator;

		LoadDataAsync();
	}

	private async void LoadDataAsync()
	{
		IsLoading = true;
		try
		{
			await LoadCountriesAsync();
			await LoadCurrenciesAsync();

			if ( Countries.Any() )
				SelectedCountry = Countries.First();
		}
		finally
		{
			IsLoading = false;
		}
	}

	public async Task LoadCountriesAsync()
	{
		var countryList = await _countryService.GetAllCountriesAsync();

		Countries.Clear();
		foreach ( var country in countryList )
		{
			Countries.Add( country );
		}

		VisibleCountryCount = Countries.Count;
		TotalCountryCount = countryList.Count;
	}

	public async Task LoadCurrenciesAsync()
	{
		var currencyList = await _currencyService.GetAllCurrenciesAsync();

		Currencies.Clear();
		foreach ( var currency in currencyList )
		{
			Currencies.Add( currency );
		}
	}

	[RelayCommand]
	private void AddCountry()
	{
		// Controleer of er al een nieuw, niet-opgeslagen record bestaat
		var existingNewCountry = Countries.FirstOrDefault(c => c.CountryId == 0);
		if ( existingNewCountry != null )
		{
			SelectedCountry = existingNewCountry;
			return;
		}

		var newCountry = new CountryModel
		{
			CountryId = 0,
			CountryCode = string.Empty,
			CountryName = string.Empty,
			CountryCurrencySymbol = null
		};

		Countries.Add( newCountry );
		SelectedCountry = newCountry;
	}

	[RelayCommand]
	private async Task SaveCountryAsync()
	{
		if ( SelectedCountry == null )
			return;

		var validation = await _countryValidator.ValidateAsync( SelectedCountry );

		if ( !validation.IsValid )
		{
			MessageBox.Show(
				string.Join( "\n", validation.Errors ),
				Lang.ExportValidationMessageTitle,
				MessageBoxButton.OK,
				MessageBoxImage.Warning );

			return;
		}

		IsSaving = true;

		// Save the selected record
		int countryId = SelectedCountry?.CountryId ?? 0;
		string? countryCode = SelectedCountry?.CountryCode;

		try
		{
			if ( countryId == 0 )
				await SaveNewCountryAsync();
			else
				await UpdateExistingCountryAsync();

			await LoadCountriesAsync();

			SelectedCountry = Countries.FirstOrDefault( c =>
				( countryId > 0 && c.CountryId == countryId ) ||
				( !string.IsNullOrWhiteSpace( countryCode ) && c.CountryCode == countryCode ) );
		}
		catch ( Exception ex )
		{
			ShowErrorMessage( $"{Lang.ExportValidationCountrySaveError}: {ex.Message}" );
		}
		finally
		{
			IsSaving = false;
		}
	}
	
	private async Task SaveNewCountryAsync()
	{
		var queryParameters = new Dictionary<string, object?>
		{
			{ $"@{DBNames.CountryFieldNameName}", SelectedCountry!.CountryName?.Trim() },
			{ $"@{DBNames.CountryFieldNameCode}", SelectedCountry.CountryCode?.Trim().ToUpper() },
			{ $"@{DBNames.CountryFieldNameCurrencyId}", SelectedCountry.CountryCurrencyId },
			{ $"@{DBNames.CountryFieldNameCurrencySymbol}", SelectedCountry.CountryCurrencySymbol }
		};

		int newId = await _countryService.InsertNewCountryAsync(queryParameters);

		SelectedCountry.CountryId = newId;

		MessageBox.Show(
			Lang.ExportValidationCountrySuccessAdded,
			Lang.ExportValidationCountrySuccessCaption,
			MessageBoxButton.OK,
			MessageBoxImage.Information );
	}

	private async Task UpdateExistingCountryAsync()
	{
		// Prepare parameters
		var queryParameters = new Dictionary<string, object?>
		{
			{ $"@{DBNames.CountryFieldNameName}", SelectedCountry!.CountryName?.Trim() },
			{ $"@{DBNames.CountryFieldNameCode}", SelectedCountry.CountryCode?.Trim().ToUpper() },
			{ $"@{DBNames.CountryFieldNameCurrencyId}", SelectedCountry.CountryCurrencyId },
			{ $"@{DBNames.CountryFieldNameCurrencySymbol}", SelectedCountry.CountryCurrencySymbol },
			{ $"@{DBNames.CountryFieldNameId}", SelectedCountry.CountryId }
		};

		// Update in database
		await _countryService.UpdateCountryAsync( queryParameters );

		MessageBox.Show( $"{Lang.ExportValidationCountrySuccessUpdated}", $"{Lang.ExportValidationCountrySuccessCaption}",
			MessageBoxButton.OK, MessageBoxImage.Information );
	}

	[RelayCommand]
	private async Task DeleteCountryAsync()
	{
		if ( SelectedCountry == null || CountryUsed )
			return;

		var result = MessageBox.Show(
			$"{Lang.ExportValidationCountryDeleteMessageText} '{SelectedCountry.CountryName}'?",
			$"{Lang.ExportValidationCountryDeleteMessageCaption}",
			MessageBoxButton.YesNo,
			MessageBoxImage.Question);

		if ( result != MessageBoxResult.Yes )
			return;

		try
		{
			await _countryService.DeleteCountryAsync(SelectedCountry.CountryId);

			// Remove from collection
			Countries.Remove( SelectedCountry );

			if ( Countries.Any() )
				SelectedCountry = Countries.FirstOrDefault();
		}
		catch ( Exception ex )
		{
			ShowErrorMessage( $"{Lang.ExportValidationCountryDeleteError}: {ex.Message}" );
		}
	}

	[RelayCommand]
	private async Task RefreshDataAsync()
	{
		IsLoading = true;
		try
		{
			await LoadCountriesAsync();
			await LoadCurrenciesAsync();

			if ( Countries.Any() )
				SelectedCountry = Countries.First();
		}
		finally
		{
			IsLoading = false;
		}
	}

	[RelayCommand]
	private async Task ImportCountriesAsync()
	{
		// Open file dialog
		var dialog = new OpenFileDialog
		{
			Filter = $"{Lang.ImportCSVFilter}",
			Title = $"{Lang.ImportCountriesTitle}"
		};

		if ( dialog.ShowDialog() != true )
			return;

		IsImporting = true;
		ImportStatus = $"{Lang.ImportGeneralMessage}...";

		try
		{
			var importResult = await Task.Run(() =>
			CsvImportService.ImportCsv(
				filePath: dialog.FileName,
				existingRecords: Countries.ToList(),
				columnMappings: CountryModel.ColumnMappings,
				uniqueProperty: nameof(CountryModel.CountryCode),
				showMessageBox: false
			));

			await ProcessImportResult( importResult, dialog.FileName );
		}
		catch ( Exception ex )
		{
			ImportStatus = $"{Lang.ImportGeneralErrorStatus}: {ex.Message}";
			MessageBox.Show( $"{Lang.ImportGeneralErrorMessage}: {ex.Message}",
				$"{Lang.ImportGeneralErrorCaption}", MessageBoxButton.OK, MessageBoxImage.Error );
		}
		finally
		{
			IsImporting = false;
		}
	}

	private async Task ProcessImportResult( Modelbouwer.Services.CsvImportResult importResult, string fileName )
	{
		if ( importResult.Imported > 0 || importResult.Updated > 0 )
		{
			await LoadCountriesAsync();
		}
	}

	private void ShowErrorMessage( string message )
	{
		MessageBox.Show( message, "Error", MessageBoxButton.OK, MessageBoxImage.Error );
	}

	partial void OnSearchTextChanged( string value )
	{
		RefreshGridFilter?.Invoke();
	}

	partial void OnSelectedCountryChanged( CountryModel? value )
	{
		if ( value == null )
			return;

		value.DefaultCurrency = Currencies
			.FirstOrDefault( c => c.CurrencyId == value.CountryCurrencyId );

		_ = CheckIfCountryIsUsedAsync( value.CountryId );
	}

	public Action? RefreshGridFilter { get; set; }

	[RelayCommand]
	private void ClearSearch()
	{
		SearchText = string.Empty;
	}

	public bool FilterCountry( object obj )
	{
		if ( obj is not CountryModel country )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		var text = SearchText.ToLower();
		return ( country.CountryCode?.Contains( text, StringComparison.CurrentCultureIgnoreCase ) == true )
			|| ( country.CountryName?.Contains( text, StringComparison.CurrentCultureIgnoreCase ) == true )
			|| ( country.CountryCurrencySymbol?.Contains( text, StringComparison.CurrentCultureIgnoreCase ) == true );
	}

	public async Task CheckIfCountryIsUsedAsync( int countryId )
	{
		if ( _countryService == null )
			return;

		CountryUsed = await _countryService.IsCountryUsedAsync( countryId );
	}

}