namespace IrrigationMaster.Mobile.Application.Interfaces;

// Abstrae la notificación al usuario (Shell.Current.DisplayAlert en MAUI) para
// que los ViewModels sean testeables sin necesitar una app MAUI corriendo.
public interface IAlertService
{
    Task ShowAsync(string title, string message);
}
