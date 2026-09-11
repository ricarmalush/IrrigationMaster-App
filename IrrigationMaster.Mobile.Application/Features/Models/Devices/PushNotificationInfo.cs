namespace IrrigationMaster.Mobile.Application.Features.Models.Devices;

// Representación agnóstica de plataforma de una notificación push -- espejo reducido de
// Plugin.Firebase.CloudMessaging.FCMNotification, para que nada fuera de la implementación de
// IPushNotificationService (en UI.Maui, la única capa con acceso a librerías nativas) conozca ese
// paquete. Data es donde viaja la ruta de deep-link (ver IPushNotificationService).
public class PushNotificationInfo
{
    public string Title { get; init; } = string.Empty;
    public string Body { get; init; } = string.Empty;
    public IDictionary<string, string> Data { get; init; } = new Dictionary<string, string>();
}
