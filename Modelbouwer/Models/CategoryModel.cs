using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Modelbouwer.Models;

public class CategoryModel : INotifyPropertyChanged
{
	public int CategoryId { get; set; }
	public int? ParentId { get; set; }
	private string? _categoryName;
	public string? CategoryName
	{
		get => _categoryName;
		set
		{
			if ( _categoryName != value )
			{
				_categoryName = value;
				OnPropertyChanged();
			}
		}
	}

	public event PropertyChangedEventHandler? PropertyChanged;
	protected void OnPropertyChanged( [CallerMemberName] string? name = null )
		=> PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( name ) );


	public ObservableCollection<CategoryModel> Children { get; set; } = [ ];

	public static readonly Dictionary<string, string[]> ColumnMappings = new()
	{
		[nameof(CategoryId)] = [ "ID" ],
		[nameof(ParentId)] = [ "Parent" ],

		[nameof(CategoryName)] = [
			"Category",
			"Category",
			"Kategorie" ]
	};
}