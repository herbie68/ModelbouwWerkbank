using Modelbouwer.Mobile.ViewModels;

namespace Modelbouwer.Mobile.Views;

public partial class TimeRegistrationPage : ContentPage
{
	private bool timerDisplayActive;
	private bool timerDisplayStarted;

	public TimeRegistrationPage()
		: this( MauiProgram.Services!.GetRequiredService<RegistrationViewModel>() )
	{
	}

	public TimeRegistrationPage( RegistrationViewModel viewModel )
	{
		InitializeComponent();
		BindingContext = viewModel;
	}

	protected override void OnAppearing()
	{
		base.OnAppearing();
		timerDisplayActive = true;
		StartTimerDisplay();
	}

	protected override void OnDisappearing()
	{
		timerDisplayActive = false;
		base.OnDisappearing();
	}

	private void StartTimerDisplay()
	{
		if ( timerDisplayStarted )
			return;

		timerDisplayStarted = true;
		Dispatcher.StartTimer( TimeSpan.FromSeconds( 1 ), () =>
		{
			if ( !timerDisplayActive )
			{
				timerDisplayStarted = false;
				return false;
			}

			if ( BindingContext is RegistrationViewModel viewModel )
				viewModel.UpdateTimerDisplay();

			return true;
		} );
	}
}