using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Modelbouwer.Mobile.Models;
using Modelbouwer.Mobile.Services;

namespace Modelbouwer.Mobile.ViewModels;

public partial class RegistrationViewModel : BaseViewModel
{
    private readonly IMobileWorkspaceService workspace;

    [ObservableProperty] private MobileProject? selectedProject;
    [ObservableProperty] private MobileProduct? selectedProduct;
    [ObservableProperty] private MobileWorkType? selectedWorkType;
    [ObservableProperty] private DateTime workDate = DateTime.Today;
    [ObservableProperty] private TimeSpan startTime = new(19, 0, 0);
    [ObservableProperty] private TimeSpan endTime = new(20, 0, 0);
    [ObservableProperty] private string workType = "Bouwen";
    [ObservableProperty] private string timeComment = string.Empty;
    [ObservableProperty] private DateTime materialDate = DateTime.Today;
    [ObservableProperty] private double materialAmount = 1;
    [ObservableProperty] private string materialComment = string.Empty;

    public RegistrationViewModel(IMobileWorkspaceService workspace)
    {
        this.workspace = workspace;
        Title = "Registratie";
    }

    public ObservableCollection<MobileProject> Projects => workspace.Projects;
    public ObservableCollection<MobileProduct> Products => workspace.Products;
    public ObservableCollection<MobileWorkType> WorkTypes => workspace.WorkTypes;
    public ObservableCollection<MobileTimeEntry> TimeEntries => workspace.TimeEntries;
    public ObservableCollection<MobileMaterialEntry> MaterialEntries => workspace.MaterialEntries;
    public ObservableCollection<MobileTimeEntry> FilteredTimeEntries { get; } = [];
    public ObservableCollection<MobileMaterialEntry> FilteredMaterialEntries { get; } = [];

    [RelayCommand]
    private Task LoadAsync()
    {
        return RunBusyAsync(async () =>
        {
            await workspace.LoadAsync();
            SelectedProject ??= Projects.FirstOrDefault();
            SelectedProduct ??= Products.FirstOrDefault();
            SelectedWorkType ??= WorkTypes.FirstOrDefault();
            RefreshFilteredRegistrations();
        }, "Database geladen.");
    }

    [RelayCommand]
    private Task SaveTimeAsync()
    {
        return RunBusyAsync(async () =>
        {
            if (SelectedProject is null || SelectedWorkType is null || EndTime <= StartTime)
            {
                StatusText = "Kies een project/werksoort en controleer de tijden.";
                return;
            }

            await workspace.AddTimeEntryAsync(new MobileTimeEntry
            {
                Project = SelectedProject,
                WorkTypeItem = SelectedWorkType,
                WorkDate = WorkDate,
                StartTime = StartTime,
                EndTime = EndTime,
                WorkType = SelectedWorkType.Name,
                Comment = TimeComment
            });

            TimeComment = string.Empty;
            RefreshFilteredRegistrations();
        }, "Urenregistratie opgeslagen.");
    }

    [RelayCommand]
    private Task SaveMaterialAsync()
    {
        return RunBusyAsync(async () =>
        {
            if (SelectedProject is null || SelectedProduct is null || MaterialAmount <= 0)
            {
                StatusText = "Kies een project/product en vul een geldige hoeveelheid in.";
                return;
            }

            await workspace.AddMaterialEntryAsync(new MobileMaterialEntry
            {
                Project = SelectedProject,
                Product = SelectedProduct,
                UsageDate = MaterialDate,
                Amount = MaterialAmount,
                Price = SelectedProduct.Price,
                Comment = MaterialComment
            });

            MaterialAmount = 1;
            MaterialComment = string.Empty;
            RefreshFilteredRegistrations();
        }, "Materiaalregistratie opgeslagen.");
    }

    partial void OnSelectedProjectChanged(MobileProject? value)
    {
        RefreshFilteredRegistrations();
    }

    private void RefreshFilteredRegistrations()
    {
        var projectId = SelectedProject?.Id;
        Replace(
            FilteredTimeEntries,
            TimeEntries
                .Where(entry => projectId is null || entry.Project?.Id == projectId)
                .OrderByDescending(entry => entry.WorkDate.Date)
                .ThenByDescending(entry => entry.StartTime)
                .ThenByDescending(entry => entry.Id)
                .Take(100));

        Replace(
            FilteredMaterialEntries,
            MaterialEntries
                .Where(entry => projectId is null || entry.Project?.Id == projectId)
                .OrderByDescending(entry => entry.UsageDate.Date)
                .ThenByDescending(entry => entry.Id)
                .Take(100));
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
            target.Add(item);
    }
}
