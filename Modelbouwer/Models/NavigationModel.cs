using System.Collections.ObjectModel;
using System.Windows.Controls;
using System.Windows.Input;

namespace Modelbouwer.Models;

public class NavigationModel
{
	public ObservableCollection<NavigationModel>? SubItems { get; set; }
	public ICommand? Command { get; set; }

	private string _navigationItem = "";
	public string NavigationItem
	{
		get => _navigationItem;
		set => _navigationItem = value;
	}

	private Image? _navigationIcon;
	public Image? NavigationIcon
	{
		get => _navigationIcon;
		set => _navigationIcon = value;
	}

	private string _navigationTooltip = "";
	public string NavigationTooltip
	{
		get => _navigationTooltip;
		set => _navigationTooltip = value;
	}
}