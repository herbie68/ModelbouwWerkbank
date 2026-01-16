using System;
using System.Collections.Generic;
using System.Text;

using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

    public class ContactTypePageViewModel : EntityPageViewModel<ContactTypeModel>
	{
	private readonly IContactTypeService _dataService;

	// SelectedContactType als type-safe alias
	public ContactTypeModel? SelectedContactType
	{
		get => SelectedItem;
		set => SelectedItem = value;
	}

	// Commands
	public IRelayCommand AddContactTypeCommand => AddCommand;
	public IAsyncRelayCommand SaveContactTypeCommand => SaveCommand;
	public IRelayCommand DeleteContactTypeCommand => DeleteCommand;
	public new IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );

	private IRelayCommand? _clearSearchCommand;

	// Constructor
	public ContactTypePageViewModel(
		IContactTypeService dataService,
		IEntityValidator<ContactTypeModel> validator
	) : base( validator )
	{
		_dataService = dataService;

		_ = LoadContactTypesAsync();
		_ = ReloadCommand.ExecuteAsync( null );
	}

	// Override SelectedItem changed om DefaultContactType te zetten
	protected override void OnSelectedItemChanged( ContactTypeModel? value )
	{
		if ( value == null )
			return;

		OnPropertyChanged( nameof( SelectedContactType ) );
		OnPropertyChanged( nameof( SelectedContactType.ContactTypeName ) );
		OnPropertyChanged( nameof( SelectedContactType.ContactTypeId ) );
	}

	// Async contacttypes laden
	private async Task LoadContactTypesAsync()
	{
		var contacttypeList = await _dataService.GetAllContactTypesAsync();

		ContactTypes.Clear();
		foreach ( var c in contacttypeList )
			ContactTypes.Add( c );
	}

	// Properties voor UI binding
	public ObservableCollection<ContactTypeModel> ContactTypes => Items;
	public int TotalContactTypeCount => TotalItemCount;
	public int VisibleContactTypeCount
	{
		get => base.VisibleItemCount;
		set => base.VisibleItemCount = value;
	}

	// Filtering
	public bool FilterContactType( object obj )
	{
		if ( obj is not ContactTypeModel contacttype )
			return false;

		if ( string.IsNullOrWhiteSpace( SearchText ) )
			return true;

		return contacttype.ContactTypeName?.Contains( SearchText, StringComparison.CurrentCultureIgnoreCase ) == true;
	}

	// Abstract overrides voor CRUD
	protected override Task<List<ContactTypeModel>> LoadItemsAsync() => _dataService.GetAllContactTypesAsync();
	protected override Task<int> InsertAsync( ContactTypeModel item ) => _dataService.InsertNewContactTypeAsync( CreateParameters( item ) );
	protected override Task UpdateAsync( ContactTypeModel item ) => _dataService.UpdateContactTypeAsync( CreateParameters( item ) );
	protected override async Task DeleteAsync( ContactTypeModel item )
	{
		if ( item == null )
			return;

		var result = MessageBox.Show(
			$"{Lang.toolbarButtonActionDeleteMessageQuestionPrefix} '{item.ContactTypeName}' {Lang.toolbarButtonActionDeleteMessageQuestionSuffix}",
			$"{Lang.toolbarButtonActionDeleteMessageButtonText}",
			MessageBoxButton.YesNo,
			MessageBoxImage.Warning
		);

		if ( result != MessageBoxResult.Yes )
			return;
		try
		{
			await _dataService.DeleteContactTypeAsync( item.ContactTypeId );
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

	protected override int GetId( ContactTypeModel item ) => item.ContactTypeId;
	protected override void SetId( ContactTypeModel item, int id ) => item.ContactTypeId = id;

	protected override ContactTypeModel CreateNewItem() => new()
	{
		ContactTypeId = 0,
		ContactTypeName = string.Empty
	};

	protected override void OnItemsLoaded()
	{
		base.OnItemsLoaded();
		OnPropertyChanged( nameof( TotalContactTypeCount ) );
	}

	// Parameter dictionary voor save
	private static Dictionary<string, object?> CreateParameters( ContactTypeModel c ) => new()
	{
		{ $"@{DBNames.ContactTypeFieldNameId}", c.ContactTypeId },
		{ $"@{DBNames.ContactTypeFieldNameName}", c.ContactTypeName?.Trim() }
	};
}
