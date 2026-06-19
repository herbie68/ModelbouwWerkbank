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
	[ObservableProperty] private bool isTimerRunning;
	[ObservableProperty] private string timerElapsedText = "00:00:00";
	[ObservableProperty] private string timerStatusText = "Geen actieve timer";
	[ObservableProperty] private DateTime materialDate = DateTime.Today;
	[ObservableProperty] private double materialAmount = 1;
	[ObservableProperty] private string materialComment = string.Empty;
	private MobileTimerSession? activeTimer;
	internal Func<DateTime> CurrentDateTime { get; set; } = () => DateTime.Now;

	public RegistrationViewModel( IMobileWorkspaceService workspace )
	{
		this.workspace = workspace;
		Title = "Registratie";
		StartTime = DateTime.Now.TimeOfDay;
		EndTime = StartTime.Add( TimeSpan.FromHours( 1 ) );
	}

	public ObservableCollection<MobileProject> Projects => workspace.Projects;
	public ObservableCollection<MobileProduct> Products => workspace.Products;
	public ObservableCollection<MobileWorkType> WorkTypes => workspace.WorkTypes;
	public ObservableCollection<MobileTimeEntry> TimeEntries => workspace.TimeEntries;
	public ObservableCollection<MobileMaterialEntry> MaterialEntries => workspace.MaterialEntries;
	public ObservableCollection<MobileTimeEntry> FilteredTimeEntries { get; } = [ ];
	public ObservableCollection<MobileMaterialEntry> FilteredMaterialEntries { get; } = [ ];
	public bool CanEditTimerSetup => !IsTimerRunning;

	[RelayCommand]
	public Task LoadAsync()
	{
		return RunBusyAsync( async () =>
		{
			await workspace.LoadAsync();
			SelectedProject ??= Projects.FirstOrDefault();
			SelectedProduct ??= Products.FirstOrDefault();
			SelectedWorkType ??= WorkTypes.FirstOrDefault();
			await LoadActiveTimerAsync();
			RefreshFilteredRegistrations();
		}, "Database geladen." );
	}

	[RelayCommand( CanExecute = nameof( CanStartTimer ) )]
	private Task StartTimerAsync()
	{
		return RunBusyAsync( async () =>
		{
			if ( SelectedProject is null || SelectedWorkType is null )
			{
				StatusText = "Kies een project en werksoort voordat je de timer start.";
				return;
			}

			var session = new MobileTimerSession
			{
				Project = SelectedProject,
				WorkTypeItem = SelectedWorkType,
				WorkDate = WorkDate.Date,
				StartTime = StartTime,
				Comment = TimeComment
			};

			await workspace.StartTimerAsync( session );
			ApplyActiveTimer( session );
			StatusText = "Timer gestart.";
		} );
	}

	[RelayCommand( CanExecute = nameof( CanStopTimer ) )]
	private Task StopTimerAsync()
	{
		return RunBusyAsync( async () =>
		{
			if ( activeTimer is null )
			{
				StatusText = "Er loopt geen timer.";
				return;
			}

			EndTime = CurrentDateTime().TimeOfDay;

			if ( EndTime <= activeTimer.StartTime )
			{
				StatusText = "De eindtijd moet later zijn dan de starttijd.";
				return;
			}

			await workspace.AddTimeEntryAsync( new MobileTimeEntry
			{
				Project = activeTimer.Project,
				WorkTypeItem = activeTimer.WorkTypeItem,
				WorkDate = activeTimer.WorkDate,
				StartTime = activeTimer.StartTime,
				EndTime = EndTime,
				WorkType = activeTimer.WorkTypeItem?.Name ?? string.Empty,
				Comment = activeTimer.Comment
			} );

			await workspace.ClearActiveTimerAsync();
			ClearActiveTimer();
			TimeComment = string.Empty;
			RefreshFilteredRegistrations();
			StatusText = "Timer gestopt en urenregistratie opgeslagen.";
		} );
	}

	[RelayCommand]
	private Task SaveTimeAsync()
	{
		return RunBusyAsync( async () =>
		{
			if ( SelectedProject is null || SelectedWorkType is null || EndTime <= StartTime )
			{
				StatusText = "Kies een project/werksoort en controleer de tijden.";
				return;
			}

			await workspace.AddTimeEntryAsync( new MobileTimeEntry
			{
				Project = SelectedProject,
				WorkTypeItem = SelectedWorkType,
				WorkDate = WorkDate,
				StartTime = StartTime,
				EndTime = EndTime,
				WorkType = SelectedWorkType.Name,
				Comment = TimeComment
			} );

			TimeComment = string.Empty;
			RefreshFilteredRegistrations();
		}, "Urenregistratie opgeslagen." );
	}

	[RelayCommand]
	private Task SaveMaterialAsync()
	{
		return RunBusyAsync( async () =>
		{
			if ( SelectedProject is null || SelectedProduct is null || MaterialAmount <= 0 )
			{
				StatusText = "Kies een project/product en vul een geldige hoeveelheid in.";
				return;
			}

			await workspace.AddMaterialEntryAsync( new MobileMaterialEntry
			{
				Project = SelectedProject,
				Product = SelectedProduct,
				UsageDate = MaterialDate,
				Amount = MaterialAmount,
				Price = SelectedProduct.Price,
				Comment = MaterialComment
			} );

			MaterialAmount = 1;
			MaterialComment = string.Empty;
			RefreshFilteredRegistrations();
		}, "Materiaalregistratie opgeslagen." );
	}

	partial void OnSelectedProjectChanged( MobileProject? value )
	{
		RefreshFilteredRegistrations();
		StartTimerCommand.NotifyCanExecuteChanged();
	}

	partial void OnSelectedWorkTypeChanged( MobileWorkType? value )
	{
		StartTimerCommand.NotifyCanExecuteChanged();
	}

	public void UpdateTimerDisplay()
	{
		if ( activeTimer is null )
		{
			TimerElapsedText = "00:00:00";
			return;
		}

		var start = activeTimer.WorkDate.Date + activeTimer.StartTime;
		var now = CurrentDateTime();
		var elapsed = now - start;
		if ( elapsed < TimeSpan.Zero )
			elapsed = TimeSpan.Zero;

		TimerElapsedText = elapsed.TotalHours >= 100
			? $"{( int ) elapsed.TotalHours:000}:{elapsed.Minutes:00}:{elapsed.Seconds:00}"
			: $"{( int ) elapsed.TotalHours:00}:{elapsed.Minutes:00}:{elapsed.Seconds:00}";
		EndTime = now.TimeOfDay;
	}

	private async Task LoadActiveTimerAsync()
	{
		var session = await workspace.GetActiveTimerAsync();
		if ( session is null )
		{
			ClearActiveTimer();
			return;
		}

		ApplyActiveTimer( session );
	}

	private void ApplyActiveTimer( MobileTimerSession session )
	{
		activeTimer = session;
		IsTimerRunning = true;
		SelectedProject = session.Project;
		SelectedWorkType = session.WorkTypeItem;
		WorkDate = session.WorkDate;
		StartTime = session.StartTime;
		TimeComment = session.Comment;
		TimerStatusText = $"{session.Project?.Name ?? "Project"} - {session.WorkTypeItem?.Name ?? "Werksoort"}";
		UpdateTimerDisplay();
		NotifyTimerCommandState();
	}

	private void ClearActiveTimer()
	{
		activeTimer = null;
		IsTimerRunning = false;
		TimerElapsedText = "00:00:00";
		TimerStatusText = "Geen actieve timer";
		NotifyTimerCommandState();
	}

	private bool CanStartTimer()
	{
		return !IsBusy && !IsTimerRunning && SelectedProject is not null && SelectedWorkType is not null;
	}

	private bool CanStopTimer()
	{
		return !IsBusy && IsTimerRunning;
	}

	partial void OnIsTimerRunningChanged( bool value )
	{
		OnPropertyChanged( nameof( CanEditTimerSetup ) );
		NotifyTimerCommandState();
	}

	protected override void OnBusyStateChanged()
	{
		NotifyTimerCommandState();
	}

	private void NotifyTimerCommandState()
	{
		StartTimerCommand.NotifyCanExecuteChanged();
		StopTimerCommand.NotifyCanExecuteChanged();
	}

	private void RefreshFilteredRegistrations()
	{
		var projectId = SelectedProject?.Id;
		Replace(
			FilteredTimeEntries,
			TimeEntries
				.Where( entry => projectId is null || entry.Project?.Id == projectId )
				.OrderByDescending( entry => entry.WorkDate.Date )
				.ThenByDescending( entry => entry.StartTime )
				.ThenByDescending( entry => entry.Id )
				.Take( 100 ) );

		Replace(
			FilteredMaterialEntries,
			MaterialEntries
				.Where( entry => projectId is null || entry.Project?.Id == projectId )
				.OrderByDescending( entry => entry.UsageDate.Date )
				.ThenByDescending( entry => entry.Id )
				.Take( 100 ) );
	}

	private static void Replace<T>( ObservableCollection<T> target, IEnumerable<T> source )
	{
		target.Clear();
		foreach ( var item in source )
			target.Add( item );
	}
}