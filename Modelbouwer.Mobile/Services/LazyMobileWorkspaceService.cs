using System.Collections.ObjectModel;
using Modelbouwer.Mobile.Models;

namespace Modelbouwer.Mobile.Services;

public sealed class LazyMobileWorkspaceService : IMobileWorkspaceService
{
    private readonly MobileDbConnectionSettings settings;
    private MySqlMobileWorkspaceService? inner;

    public ObservableCollection<MobileProject> Projects { get; } = [];
    public ObservableCollection<MobileProduct> Products { get; } = [];
    public ObservableCollection<MobileWorkType> WorkTypes { get; } = [];
    public ObservableCollection<MobileCategory> Categories { get; } = [];
    public ObservableCollection<MobileUnit> Units { get; } = [];
    public ObservableCollection<MobileTimeEntry> TimeEntries { get; } = [];
    public ObservableCollection<MobileMaterialEntry> MaterialEntries { get; } = [];

    public LazyMobileWorkspaceService(MobileDbConnectionSettings settings)
    {
        this.settings = settings;
    }

    public async Task LoadAsync()
    {
        var service = GetInner();
        await service.LoadAsync();
        CopyFromInner();
    }

    public async Task AddProjectAsync(MobileProject project)
    {
        await GetInner().AddProjectAsync(project);
        CopyFromInner();
    }

    public async Task UpdateProjectAsync(MobileProject project)
    {
        await GetInner().UpdateProjectAsync(project);
    }

    public async Task AddProductAsync(MobileProduct product)
    {
        await GetInner().AddProductAsync(product);
        CopyFromInner();
    }

    public async Task UpdateProductAsync(MobileProduct product)
    {
        await GetInner().UpdateProductAsync(product);
    }

    public async Task AddTimeEntryAsync(MobileTimeEntry entry)
    {
        await GetInner().AddTimeEntryAsync(entry);
        Replace(TimeEntries, GetInner().TimeEntries);
    }

    public async Task AddMaterialEntryAsync(MobileMaterialEntry entry)
    {
        await GetInner().AddMaterialEntryAsync(entry);
        Replace(MaterialEntries, GetInner().MaterialEntries);
    }

    private MySqlMobileWorkspaceService GetInner()
    {
        return inner ??= new MySqlMobileWorkspaceService(settings);
    }

    private void CopyFromInner()
    {
        if (inner is null)
            return;

        Replace(Projects, inner.Projects);
        Replace(Products, inner.Products);
        Replace(WorkTypes, inner.WorkTypes);
        Replace(Categories, inner.Categories);
        Replace(Units, inner.Units);
        Replace(TimeEntries, inner.TimeEntries);
        Replace(MaterialEntries, inner.MaterialEntries);
    }

    private static void Replace<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();
        foreach (var item in source)
            target.Add(item);
    }
}
