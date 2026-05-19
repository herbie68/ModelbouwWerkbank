using System.Collections.ObjectModel;
using Modelbouwer.Mobile.Models;

namespace Modelbouwer.Mobile.Services;

public sealed class DesignTimeWorkspaceService : IMobileWorkspaceService
{
    private int nextProjectId = 3;
    private int nextProductId = 4;
    private int nextTimeEntryId = 2;
    private int nextMaterialEntryId = 2;
    private MobileTimerSession? activeTimer;

    public ObservableCollection<MobileProject> Projects { get; } =
    [
        new() { Id = 1, Code = "SPITFIRE", Name = "Spitfire Mk.IX", StartDate = DateTime.Today.AddDays(-18) },
        new() { Id = 2, Code = "BR-52", Name = "BR 52 locomotief", StartDate = DateTime.Today.AddDays(-42), IsClosed = false }
    ];

    public ObservableCollection<MobileProduct> Products { get; } =
    [
        new() { Id = 1, CategoryId = 1, UnitId = 1, Code = "TAM-87038", Name = "Extra Thin Cement", Category = "Lijm", Unit = "ml", CurrentInventory = 31, MinimalStock = 10, Price = 4.95 },
        new() { Id = 2, CategoryId = 2, UnitId = 1, Code = "VAL-71.057", Name = "Black Model Air", Category = "Verf", Unit = "ml", CurrentInventory = 12, MinimalStock = 5, Price = 2.85 },
        new() { Id = 3, CategoryId = 3, UnitId = 2, Code = "EVER-218", Name = "Styreen strip 1.5mm", Category = "Materiaal", Unit = "st", CurrentInventory = 8, MinimalStock = 3, Price = 3.40 }
    ];

    public ObservableCollection<MobileWorkType> WorkTypes { get; } =
    [
        new() { Id = 1, Name = "Bouwen", DisplayName = "Bouwen" },
        new() { Id = 2, ParentId = 1, Name = "Schilderen", DisplayName = "  Schilderen" }
    ];

    public ObservableCollection<MobileCategory> Categories { get; } =
    [
        new() { Id = 1, Name = "Lijm", DisplayName = "Lijm" },
        new() { Id = 2, Name = "Verf", DisplayName = "Verf" },
        new() { Id = 3, Name = "Materiaal", DisplayName = "Materiaal" }
    ];

    public ObservableCollection<MobileUnit> Units { get; } =
    [
        new() { Id = 1, Name = "ml" },
        new() { Id = 2, Name = "st" }
    ];

    public ObservableCollection<MobileTimeEntry> TimeEntries { get; } = [];
    public ObservableCollection<MobileMaterialEntry> MaterialEntries { get; } = [];

    public DesignTimeWorkspaceService()
    {
        TimeEntries.Add(new MobileTimeEntry
        {
            Id = 1,
            Project = Projects[0],
            WorkDate = DateTime.Today,
            StartTime = new TimeSpan(19, 30, 0),
            EndTime = new TimeSpan(21, 15, 0),
            WorkTypeItem = WorkTypes[0],
            WorkType = "Bouwen",
            Comment = "Cockpit dryfit."
        });

        MaterialEntries.Add(new MobileMaterialEntry
        {
            Id = 1,
            Project = Projects[0],
            Product = Products[0],
            UsageDate = DateTime.Today,
            Amount = 0.5,
            Price = Products[0].Price,
            Comment = "Lijmwerk cockpit."
        });
    }

    public Task LoadAsync()
    {
        return Task.CompletedTask;
    }

    public Task AddProjectAsync(MobileProject project)
    {
        project.Id = nextProjectId++;
        Projects.Add(project);
        return Task.CompletedTask;
    }

    public Task UpdateProjectAsync(MobileProject project)
    {
        return Task.CompletedTask;
    }

    public Task AddProductAsync(MobileProduct product)
    {
        product.Id = nextProductId++;
        Products.Add(product);
        return Task.CompletedTask;
    }

    public Task UpdateProductAsync(MobileProduct product)
    {
        return Task.CompletedTask;
    }

    public Task AddTimeEntryAsync(MobileTimeEntry entry)
    {
        entry.Id = nextTimeEntryId++;
        TimeEntries.Insert(0, entry);
        return Task.CompletedTask;
    }

    public Task AddMaterialEntryAsync(MobileMaterialEntry entry)
    {
        entry.Id = nextMaterialEntryId++;
        MaterialEntries.Insert(0, entry);
        return Task.CompletedTask;
    }

    public Task<MobileTimerSession?> GetActiveTimerAsync()
    {
        return Task.FromResult(activeTimer);
    }

    public Task StartTimerAsync(MobileTimerSession session)
    {
        if (activeTimer is not null)
            throw new InvalidOperationException("Er loopt al een timer.");

        activeTimer = session;
        return Task.CompletedTask;
    }

    public Task ClearActiveTimerAsync()
    {
        activeTimer = null;
        return Task.CompletedTask;
    }
}
