using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Services;

public class ShellAlertService : IAlertService
{
    public Task ShowAsync(string title, string message) => Shell.Current.DisplayAlert(title, message, "OK");
}
