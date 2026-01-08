using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using System.Collections.ObjectModel;

using Modelbouwer.Interfaces;

using RelayCommand = CommunityToolkit.Mvvm.Input.RelayCommand;

namespace Modelbouwer.ViewModels;

public abstract partial class EntityPageViewModel<T> : ObservableObject
{
	protected readonly IEntityValidator<T> Validator;

	protected EntityPageViewModel( IEntityValidator<T> validator )
	{
		Validator = validator;
	}

	// Collections
	public ObservableCollection<T> Items { get; } = [ ];

	// State
	[ObservableProperty] protected bool _isLoading;
	[ObservableProperty] protected bool _isSaving;

	[ObservableProperty] protected string _searchText = string.Empty;

	[ObservableProperty] public int _visibleItemCount;

	public int TotalItemCount => Items.Count;

	// Grid / filtering
	public Action? RefreshGridFilter { get; set; }

	// Commands (EXACT hetzelfde patroon als nu)
	public IRelayCommand AddCommand => _addCommand ??= new RelayCommand( Add );
	public IAsyncRelayCommand SaveCommand => _saveCommand ??= new AsyncRelayCommand( SaveAsync );
	public IAsyncRelayCommand ReloadCommand => _reloadCommand ??= new AsyncRelayCommand( ReloadAsync );
	public IAsyncRelayCommand DeleteCommand => _deleteCommand ??= new AsyncRelayCommand( DeleteItemAsync );
	public IRelayCommand ClearSearchCommand => _clearSearchCommand ??= new RelayCommand( ClearSearch );

	private IRelayCommand? _addCommand;
	private IRelayCommand? _clearSearchCommand;
	private IAsyncRelayCommand? _deleteCommand;
	private IAsyncRelayCommand? _saveCommand;
	private IAsyncRelayCommand? _reloadCommand;

	// Abstract hooks
	protected abstract Task<List<T>> LoadItemsAsync();
	protected abstract Task<int> InsertAsync( T item );
	protected abstract Task UpdateAsync( T item );
	protected abstract Task DeleteAsync( T item );
	protected abstract int GetId( T item );
	protected abstract void SetId( T item, int id );

	protected abstract T CreateNewItem();

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

	protected virtual void OnSelectedItemChanged( T? value )
	{
	}

	// ---- default implementations ----

	private void Add()
	{
		var existing = Items.FirstOrDefault( i => GetId( i ) == 0 );
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

		var validation = await Validator.ValidateAsync( SelectedItem );
		if ( !validation.IsValid )
			throw new InvalidOperationException(
				string.Join( "\n", validation.Errors ) );

		IsSaving = true;
		try
		{
			if ( GetId( SelectedItem ) == 0 )
			{
				var id = await InsertAsync( SelectedItem );
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

	private async Task DeleteItemAsync()
	{
		if ( SelectedItem != null )
			await DeleteAsync( SelectedItem );
	}

	partial void OnSearchTextChanged( string value )
	{
		RefreshGridFilter?.Invoke();
		OnPropertyChanged( nameof( VisibleItemCount ) );
	}

	private void ClearSearch()
	{
		SearchText = string.Empty;
		RefreshGridFilter?.Invoke();
	}

	protected virtual void OnItemsLoaded()
	{
		OnPropertyChanged( nameof( TotalItemCount ) );
	}
}
