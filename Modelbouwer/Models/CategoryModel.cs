namespace Modelbouwer.Models;

public class CategoryModel
{
	public int CategoryId { get; set; }
	public int? ParentId { get; set; }
	public string CategoryName { get; set; } = string.Empty;

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