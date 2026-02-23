namespace Modelbouwer.Models;

public partial class CategoryModel : ObservableObject
{
	[ObservableProperty] public int _categoryId;

	[ObservableProperty] public int? _parentId;

	[ObservableProperty] public CategoryModel? _parent = null;

	[ObservableProperty] public string _categoryName = string.Empty;

	public ObservableCollection<CategoryModel> Children { get; set; } = [ ];

	public static readonly Dictionary<string, string[]> ColumnMappings = new()
	{
		[nameof(CategoryId)] = [ "ID" ],
		[nameof(ParentId)] = [ "Parent" ],

		[nameof(CategoryName)] = [
			"Categorie",
			"Category",
			"Kategorie" ]
	};
}