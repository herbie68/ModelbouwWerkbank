namespace Modelbouwer.Models;

public partial class ProductSelectionNodeModel : ObservableObject
{
	[ObservableProperty] private string _displayName = string.Empty;
	[ObservableProperty] private int _categoryId;
	[ObservableProperty] private ProductModel? _product;

	public bool IsProduct => Product != null;
	public bool IsCategory => Product == null;
	public ObservableCollection<ProductSelectionNodeModel> Children { get; } = [ ];
}