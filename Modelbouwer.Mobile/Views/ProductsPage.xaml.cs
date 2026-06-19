using Modelbouwer.Mobile.ViewModels;

namespace Modelbouwer.Mobile.Views;

public partial class ProductsPage : ContentPage
{
	public ProductsPage()
		: this( MauiProgram.Services!.GetRequiredService<ProductsViewModel>() )
	{
	}

	public ProductsPage( ProductsViewModel viewModel )
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}