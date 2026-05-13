using Modelbouwer.Mobile.ViewModels;

namespace Modelbouwer.Mobile.Views;

public partial class TimeRegistrationPage : ContentPage
{
    public TimeRegistrationPage()
        : this(MauiProgram.Services!.GetRequiredService<RegistrationViewModel>())
    {
    }

    public TimeRegistrationPage(RegistrationViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
