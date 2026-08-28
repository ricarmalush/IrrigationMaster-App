using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Services;

public class ShellAlertService : IAlertService
{
    public Task ShowAsync(string title, string message) => Shell.Current.DisplayAlert(title, message, "OK");

    public Task<bool> ShowConfirmAsync(string title, string message, string acceptText, string cancelText)
        => Shell.Current.DisplayAlert(title, message, acceptText, cancelText);
}
