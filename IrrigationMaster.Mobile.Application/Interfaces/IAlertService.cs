namespace IrrigationMaster.Mobile.Application.Interfaces;

// Abstrae la notificación al usuario (Shell.Current.DisplayAlert en MAUI) para
// que los ViewModels sean testeables sin necesitar una app MAUI corriendo.
public interface IAlertService
{
    Task ShowAsync(string title, string message);

    // Diálogo de confirmación Sí/No -- true si el usuario acepta. Para acciones que necesitan
    // una confirmación explícita antes de ejecutarse (p. ej. regenerar el código de invitación:
    // el código anterior deja de servir de inmediato).
    Task<bool> ShowConfirmAsync(string title, string message, string acceptText, string cancelText);
}
