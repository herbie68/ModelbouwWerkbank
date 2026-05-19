using System.Collections.ObjectModel;
using Modelbouwer.Mobile.Models;
using Modelbouwer.Mobile.Services;
using Modelbouwer.Mobile.ViewModels;

namespace Modelbouwer.UnitTests.ViewModels;

[TestClass]
public sealed class MobileRegistrationTimerViewModelTests
{
	[TestMethod]
	public async Task LoadAsync_NotifiesStartTimerCommandAfterBusyStateEnds()
	{
		var workspace = new FakeMobileWorkspaceService();
		var viewModel = new RegistrationViewModel(workspace);
		List<bool> canExecuteStates = [];
		viewModel.StartTimerCommand.CanExecuteChanged += (_, _) =>
		{
			canExecuteStates.Add(viewModel.StartTimerCommand.CanExecute(null));
		};

		await viewModel.LoadAsync();

		Assert.IsTrue(viewModel.StartTimerCommand.CanExecute(null));
		Assert.IsTrue(canExecuteStates.Count > 0);
		Assert.IsTrue(canExecuteStates[^1]);
	}

	[TestMethod]
	public async Task StartTimerCommand_StoresRunningSessionAndDisablesStartingAnotherTimer()
	{
		var workspace = new FakeMobileWorkspaceService();
		var viewModel = new RegistrationViewModel(workspace)
		{
			SelectedProject = workspace.Projects[0],
			SelectedWorkType = workspace.WorkTypes[0],
			WorkDate = new DateTime(2026, 5, 19),
			StartTime = new TimeSpan(9, 15, 0)
		};

		await viewModel.StartTimerCommand.ExecuteAsync(null);

		Assert.IsTrue(viewModel.IsTimerRunning);
		Assert.IsFalse(viewModel.StartTimerCommand.CanExecute(null));
		Assert.AreEqual(new TimeSpan(9, 15, 0), workspace.ActiveTimer?.StartTime);
		Assert.AreEqual(workspace.Projects[0], workspace.ActiveTimer?.Project);
		Assert.AreEqual(workspace.WorkTypes[0], workspace.ActiveTimer?.WorkTypeItem);
	}

	[TestMethod]
	public async Task StopTimerCommand_SavesTimeEntryWithCurrentEndTimeAndClearsRunningSession()
	{
		var workspace = new FakeMobileWorkspaceService();
		var viewModel = new RegistrationViewModel(workspace)
		{
			CurrentDateTime = () => new DateTime(2026, 5, 19, 10, 45, 0)
		};

		await workspace.StartTimerAsync(new MobileTimerSession
		{
			Project = workspace.Projects[0],
			WorkTypeItem = workspace.WorkTypes[0],
			WorkDate = new DateTime(2026, 5, 19),
			StartTime = new TimeSpan(9, 15, 0),
			Comment = "Timer test"
		});
		await viewModel.LoadAsync();

		await viewModel.StopTimerCommand.ExecuteAsync(null);

		Assert.IsFalse(viewModel.IsTimerRunning);
		Assert.IsNull(workspace.ActiveTimer);
		Assert.AreEqual(1, workspace.TimeEntries.Count);
		Assert.AreEqual(new TimeSpan(9, 15, 0), workspace.TimeEntries[0].StartTime);
		Assert.AreEqual(new TimeSpan(10, 45, 0), workspace.TimeEntries[0].EndTime);
	}

	private sealed class FakeMobileWorkspaceService : IMobileWorkspaceService
	{
		private int nextTimeEntryId = 1;

		public ObservableCollection<MobileProject> Projects { get; } =
		[
			new() { Id = 1, Name = "Test project" }
		];

		public ObservableCollection<MobileProduct> Products { get; } = [];
		public ObservableCollection<MobileWorkType> WorkTypes { get; } =
		[
			new() { Id = 2, Name = "Bouwen", DisplayName = "Bouwen" }
		];

		public ObservableCollection<MobileCategory> Categories { get; } = [];
		public ObservableCollection<MobileUnit> Units { get; } = [];
		public ObservableCollection<MobileTimeEntry> TimeEntries { get; } = [];
		public ObservableCollection<MobileMaterialEntry> MaterialEntries { get; } = [];
		public MobileTimerSession? ActiveTimer { get; private set; }

		public Task LoadAsync() => Task.CompletedTask;
		public Task AddProjectAsync(MobileProject project) => Task.CompletedTask;
		public Task UpdateProjectAsync(MobileProject project) => Task.CompletedTask;
		public Task AddProductAsync(MobileProduct product) => Task.CompletedTask;
		public Task UpdateProductAsync(MobileProduct product) => Task.CompletedTask;

		public Task AddTimeEntryAsync(MobileTimeEntry entry)
		{
			entry.Id = nextTimeEntryId++;
			TimeEntries.Insert(0, entry);
			return Task.CompletedTask;
		}

		public Task AddMaterialEntryAsync(MobileMaterialEntry entry) => Task.CompletedTask;

		public Task<MobileTimerSession?> GetActiveTimerAsync() => Task.FromResult(ActiveTimer);

		public Task StartTimerAsync(MobileTimerSession session)
		{
			if (ActiveTimer is not null)
				throw new InvalidOperationException("Er loopt al een timer.");

			ActiveTimer = session;
			return Task.CompletedTask;
		}

		public Task ClearActiveTimerAsync()
		{
			ActiveTimer = null;
			return Task.CompletedTask;
		}
	}
}
