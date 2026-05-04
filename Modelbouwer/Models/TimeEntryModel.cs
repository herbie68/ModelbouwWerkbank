using System.ComponentModel;

namespace Modelbouwer.Models;

public partial class TimeEntryModel : ObservableObject
{
	public enum RecordState
	{
		Unchanged,
		Added,
		Modified,
		Deleted
	}

	[ObservableProperty] private int _timeId;
	[ObservableProperty] private int _projectId;
	[ObservableProperty] private string? _projectName;
	[ObservableProperty] private int _worktypeId;
	[ObservableProperty] private string? _worktypeName;
	[ObservableProperty] private DateTime _workDate = DateTime.Today;
	[ObservableProperty] private string _startTime = "09:00";
	[ObservableProperty] private string _endTime = "10:00";
	[ObservableProperty] private string? _comment;

	private RecordState _state = RecordState.Unchanged;
	public RecordState State
	{
		get => _state;
		set => SetProperty( ref _state, value );
	}

	public string StatusMarker => State == RecordState.Unchanged ? string.Empty : "*";

	public double WorkedMinutes
	{
		get
		{
			if ( !TimeSpan.TryParse( StartTime, CultureInfo.CurrentCulture, out var start ) ||
				!TimeSpan.TryParse( EndTime, CultureInfo.CurrentCulture, out var end ) ||
				end <= start )
				return 0;

			return ( end - start ).TotalMinutes;
		}
	}

	public string WorkedTime => TimeSpan.FromMinutes( WorkedMinutes ).ToString( @"hh\:mm", CultureInfo.CurrentCulture );

	public TimeEntryModel()
	{
		PropertyChanged += TimeEntryModel_PropertyChanged;
	}

	private void TimeEntryModel_PropertyChanged( object? sender, PropertyChangedEventArgs e )
	{
		if ( e.PropertyName == nameof( State ) ||
			e.PropertyName == nameof( StatusMarker ) ||
			e.PropertyName == nameof( WorkedMinutes ) ||
			e.PropertyName == nameof( WorkedTime ) )
			return;

		if ( e.PropertyName == nameof( StartTime ) || e.PropertyName == nameof( EndTime ) )
		{
			OnPropertyChanged( nameof( WorkedMinutes ) );
			OnPropertyChanged( nameof( WorkedTime ) );
		}

		if ( State == RecordState.Unchanged )
		{
			_state = RecordState.Modified;
			OnPropertyChanged( nameof( State ) );
			OnPropertyChanged( nameof( StatusMarker ) );
		}
	}
}
