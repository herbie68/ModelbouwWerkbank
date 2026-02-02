using CommunityToolkit.Mvvm.Input;

namespace Modelbouwer.ViewModels;

public abstract partial class EntityPageViewModel<T> : ObservableObject
{
	protected readonly IEntityValidator<T> Validator;

	protected EntityPageViewModel( IEntityValidator<T> validator )
	{
		Validator = validator;
	}

	// -----------------------------
	// Collections & selection
	// -----------------------------
	public ObservableCollection<T> Items { get; } = new();

	private T? _selectedItem;
	public T? SelectedItem
	{
		get => _selectedItem;
		set
		{
			if ( SetProperty( ref _selectedItem, value ) )
			{
				OnSelectedItemChanged( value );
			}
		}
	}

	// -----------------------------
	// State
	// -----------------------------
	[ObservableProperty] protected bool _isLoading;
	[ObservableProperty] protected bool _isSaving;
	[ObservableProperty] protected string _searchText = string.Empty;
	[ObservableProperty] protected int _visibleItemCount;

	public int TotalItemCount => Items.Count;

	// Grid filtering hook
	public Action? RefreshGridFilter { get; set; }

	// -----------------------------
	// Commands
	// -----------------------------
	public IRelayCommand AddCommand => _addCommand ??= new RelayCommand( Add );
	public IRelayCommand DeleteCommand => _deleteCommand ??= new RelayCommand( Delete );
	public IAsyncRelayCommand SaveCommand => _saveCommand ??= new AsyncRelayCommand( SaveAsync );
	public IAsyncRelayCommand ReloadCommand => _reloadCommand ??= new AsyncRelayCommand( ReloadAsync );
	public IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );

	private IRelayCommand? _addCommand;
	private IRelayCommand? _addImageCommand;
	private IRelayCommand? _deleteCommand;
	private IAsyncRelayCommand? _saveCommand;
	private IAsyncRelayCommand? _reloadCommand;
	private IRelayCommand? _clearSearchCommand;

	// -----------------------------
	// Abstract hooks for child
	// -----------------------------
	protected abstract Task<List<T>> LoadItemsAsync();
	protected abstract Task<int> InsertAsync( T item );
	protected abstract Task UpdateAsync( T item );
	protected abstract Task DeleteAsync( T item );
	protected abstract int GetId( T item );
	protected abstract void SetId( T item, int id );
	protected abstract T CreateNewItem();

	protected virtual void OnSelectedItemChanged( T? value ) { }

	// -----------------------------
	// Default implementations
	// -----------------------------
	private void Add()
	{
		var existing = Items.FirstOrDefault(i => GetId(i) == 0);
		if ( existing != null )
		{
			SelectedItem = existing;
			return;
		}

		var item = CreateNewItem();
		Items.Add( item );
		SelectedItem = item;
	}

	private async Task SaveAsync()
	{
		if ( SelectedItem == null )
			return;

		var validation = await Validator.ValidateAsync(SelectedItem);
		if ( !validation.IsValid )
		{
			var result = MessageBox.Show(
				string.Join( "\n", validation.Errors ),
				Lang.ExportValidationCategoryNameExists,
				MessageBoxButton.OK,
				MessageBoxImage.Error, MessageBoxResult.Abort );
			return;
		}

		IsSaving = true;
		try
		{
			if ( GetId( SelectedItem ) == 0 )
			{
				var id = await InsertAsync(SelectedItem);
				SetId( SelectedItem, id );
			}
			else
			{
				await UpdateAsync( SelectedItem );
			}

			await ReloadAsync();
		}
		finally
		{
			IsSaving = false;
		}
	}

	private async Task ReloadAsync()
	{
		IsLoading = true;
		try
		{
			var items = await LoadItemsAsync();
			Items.Clear();
			foreach ( var item in items )
				Items.Add( item );

			if ( Items.Any() )
				SelectedItem = Items.First();

			OnItemsLoaded();
		}
		finally
		{
			IsLoading = false;
		}
	}

	private async void Delete()
	{
		if ( SelectedItem == null )
			return;

		try
		{
			await DeleteAsync( SelectedItem );
			await ReloadAsync();
		}
		catch ( Exception ex )
		{
			// fallback logging, mocht er toch iets misgaan
			MessageBox.Show( $"{Lang.generalMessageboxDeleteError}: {ex.Message}" );
		}
	}


	partial void OnSearchTextChanged( string value )
	{
		RefreshGridFilter?.Invoke();
		OnPropertyChanged( nameof( _visibleItemCount ) );
	}

	protected virtual void OnItemsLoaded()
	{
		OnPropertyChanged( nameof( TotalItemCount ) );
	}
}
