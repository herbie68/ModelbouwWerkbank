namespace Modelbouwer.Views;

public partial class StockOrderView : UserControl
{
	public StockOrderView( StockOrderViewModel viewModel )
	{
		InitializeComponent();
		ApplyCurrentCultureFormatting();
		DataContext = viewModel;
	}

	private void ApplyCurrentCultureFormatting()
	{
		var culture = CultureInfo.CurrentCulture;
		Language = System.Windows.Markup.XmlLanguage.GetLanguage( culture.IetfLanguageTag );

		foreach ( var column in OrderLinesGrid.Columns.OfType<Syncfusion.UI.Xaml.Grid.GridNumericColumn>()
			.Concat( AvailableProductsGrid.Columns.OfType<Syncfusion.UI.Xaml.Grid.GridNumericColumn>() ) )
		{
			column.NumberDecimalSeparator = culture.NumberFormat.NumberDecimalSeparator;
			column.NumberGroupSeparator = culture.NumberFormat.NumberGroupSeparator;
			column.NumberGroupSizes = new System.Windows.Media.Int32Collection( culture.NumberFormat.NumberGroupSizes );
		}
	}
}