using System.Collections.ObjectModel;

using Modelbouwer.Mobile.Models;

namespace Modelbouwer.Mobile.Services;

public interface IMobileWorkspaceService
{
	ObservableCollection<MobileProject> Projects { get; }
	ObservableCollection<MobileProduct> Products { get; }
	ObservableCollection<MobileWorkType> WorkTypes { get; }
	ObservableCollection<MobileCategory> Categories { get; }
	ObservableCollection<MobileUnit> Units { get; }
	ObservableCollection<MobileTimeEntry> TimeEntries { get; }
	ObservableCollection<MobileMaterialEntry> MaterialEntries { get; }

	Task LoadAsync();
	Task AddProjectAsync( MobileProject project );
	Task UpdateProjectAsync( MobileProject project );
	Task AddProductAsync( MobileProduct product );
	Task UpdateProductAsync( MobileProduct product );
	Task AddTimeEntryAsync( MobileTimeEntry entry );
	Task AddMaterialEntryAsync( MobileMaterialEntry entry );
	Task<MobileTimerSession?> GetActiveTimerAsync();
	Task StartTimerAsync( MobileTimerSession session );
	Task ClearActiveTimerAsync();
}