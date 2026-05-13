using Modelbouwer.Mobile.ViewModels;

namespace Modelbouwer.Mobile.Views;

public partial class ProjectsPage : ContentPage
{
    public ProjectsPage()
        : this(MauiProgram.Services!.GetRequiredService<ProjectsViewModel>())
    {
    }

    public ProjectsPage(ProjectsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
