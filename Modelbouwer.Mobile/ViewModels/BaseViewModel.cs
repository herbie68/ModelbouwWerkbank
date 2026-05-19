using CommunityToolkit.Mvvm.ComponentModel;

namespace Modelbouwer.Mobile.ViewModels;

public abstract partial class BaseViewModel : ObservableObject
{
    [ObservableProperty] private string title = string.Empty;
    [ObservableProperty] private string statusText = "Gereed";
    [ObservableProperty] private bool isBusy;

    partial void OnIsBusyChanged(bool value)
    {
        OnBusyStateChanged();
    }

    protected virtual void OnBusyStateChanged()
    {
    }

    protected async Task RunBusyAsync(Func<Task> action, string? successText = null)
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;
            StatusText = "Bezig...";
            await action();
            if (!string.IsNullOrWhiteSpace(successText))
                StatusText = successText;
        }
        catch (Exception ex)
        {
            StatusText = $"Databasefout: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
