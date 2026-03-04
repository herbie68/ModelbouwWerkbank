using System.ComponentModel;

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

	public T? SelectedItem
	{
		get;
		set
		{
			if ( !EqualityComparer<T?>.Default.Equals( field, value ) )
			{
				var oldValue = field;

				if ( SetProperty( ref field, value ) )
				{
					OnSelectedItemChanged( oldValue, value );
				}
			}
		}
	}

	// -----------------------------
	// State
	// -----------------------------
	[ObservableProperty] protected bool _isLoading;
	[ObservableProperty] protected bool _isSaving;
	[ObservableProperty] protected string _searchText = string.Empty;
	[ObservableProperty]
	private bool hasUnsavedChanges;

	public int TotalItemCount => Items.Count;

	private int _visibleItemCount;
	public int VisibleItemCount
	{
		get => _visibleItemCount;
		set => SetProperty( ref _visibleItemCount, value );
	}

	// Grid filtering hook
	public Action? RefreshGridFilter { get; set; }

	// -----------------------------
	// Commands
	// -----------------------------
	public IRelayCommand AddCommand => _addCommand ??= new RelayCommand( Add );
	public IRelayCommand AddContactCommand => _addContactCommand ??= new RelayCommand( Add );
	public IRelayCommand DeleteCommand => _deleteCommand ??= new RelayCommand( Delete );
	public IRelayCommand DeleteContactCommand => _deleteContactCommand ??= new RelayCommand( Delete );
	public IAsyncRelayCommand SaveCommand => _saveCommand ??= new AsyncRelayCommand( SaveAsync );
	public IAsyncRelayCommand SaveContactCommand => _saveContactCommand ??= new AsyncRelayCommand( SaveAsync );
	public IAsyncRelayCommand ReloadCommand => _reloadCommand ??= new AsyncRelayCommand( ReloadAsync );
	public IAsyncRelayCommand ReloadContactsCommand => _reloadContactsCommand ??= new AsyncRelayCommand( ReloadAsync );
	public IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( () => SearchText = string.Empty );

	private IRelayCommand? _addCommand;
	private IRelayCommand? _addContactCommand;
	private IRelayCommand? _addImageCommand;
	private IRelayCommand? _deleteCommand;
	private IRelayCommand? _deleteContactCommand;
	private IAsyncRelayCommand? _saveCommand;
	private IAsyncRelayCommand? _saveContactCommand;
	private IAsyncRelayCommand? _reloadCommand;
	private IAsyncRelayCommand? _reloadContactsCommand;
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

	protected virtual void OnSelectedItemChanged( T? oldValue, T? newValue )
	{
		if ( oldValue is INotifyPropertyChanged oldNpc )
			oldNpc.PropertyChanged -= Item_PropertyChanged;

		if ( newValue is INotifyPropertyChanged newNpc )
			newNpc.PropertyChanged += Item_PropertyChanged;

		HasUnsavedChanges = false;
	}

	private void Item_PropertyChanged( object? sender, PropertyChangedEventArgs e )
	{
		HasUnsavedChanges = true;
	}

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

		HasUnsavedChanges = true;
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

		HasUnsavedChanges = false;
	}

	protected async Task ReloadAsync()
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
