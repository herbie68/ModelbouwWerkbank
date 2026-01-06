using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

using Modelbouwer.ViewModels;

namespace Modelbouwer;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
	public MainWindow( NavigationViewModel navigationViewModel )
	{
		InitializeComponent();
		DataContext = navigationViewModel;
	}
}