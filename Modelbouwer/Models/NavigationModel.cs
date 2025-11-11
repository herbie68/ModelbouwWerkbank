namespace Modelbouwer.Models;

public class NavigationModel
{
	public ObservableCollection<NavigationModel>? SubItems { get; set; }
	public ICommand? Command { get; set; }

	private string navigationItem = "";
	public string NavigationItem
	{
		get { return navigationItem; }
		set { navigationItem = value; }
	}

	private object navigationIcon = "";
	public object NavigationIcon
	{
		get { return navigationIcon; }
		set { navigationIcon = value; }
	}

	private object navigationTooltip = "";
	public object NavigationTooltip
	{
		get { return navigationTooltip; }
		set { navigationTooltip = value; }
	}
}
