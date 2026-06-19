using System.Collections.ObjectModel;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Modelbouwer.Mobile.Models;
using Modelbouwer.Mobile.Services;

namespace Modelbouwer.Mobile.ViewModels;

public partial class ProjectsViewModel : BaseViewModel
{
	private readonly IMobileWorkspaceService workspace;

	[ObservableProperty] private MobileProject? selectedProject;

	public ProjectsViewModel( IMobileWorkspaceService workspace )
	{
		this.workspace = workspace;
		Title = "Projecten";
	}

	public ObservableCollection<MobileProject> Projects => workspace.Projects;

	[RelayCommand]
	private Task LoadAsync()
	{
		return RunBusyAsync( async () =>
		{
			await workspace.LoadAsync();
			SelectedProject ??= Projects.FirstOrDefault();
		}, "Database geladen." );
	}

	[RelayCommand]
	private async Task NewProjectAsync()
	{
		var project = new MobileProject { Code = "NIEUW", Name = "Nieuw project", StartDate = DateTime.Today };
		await RunBusyAsync( async () =>
		{
			await workspace.AddProjectAsync( project );
			SelectedProject = project;
		}, "Nieuw project toegevoegd." );
	}

	[RelayCommand]
	private Task SaveProjectAsync()
	{
		return RunBusyAsync( async () =>
		{
			if ( SelectedProject is null )
			{
				StatusText = "Kies een project.";
				return;
			}

			await workspace.UpdateProjectAsync( SelectedProject );
		}, "Projectgegevens bijgewerkt." );
	}
}