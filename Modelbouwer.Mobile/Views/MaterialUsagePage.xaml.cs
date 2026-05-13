using Modelbouwer.Mobile.ViewModels;

namespace Modelbouwer.Mobile.Views;

public partial class MaterialUsagePage : ContentPage
{
    public MaterialUsagePage()
        : this(MauiProgram.Services!.GetRequiredService<RegistrationViewModel>())
    {
    }

    public MaterialUsagePage(RegistrationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
