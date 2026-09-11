using IrrigationMaster.Mobile.Application.Features.Models.Devices;

namespace IrrigationMaster.Mobile.Application.Interfaces;

// Abstracción sobre Plugin.Firebase.CloudMessaging (Android/iOS únicamente -- sin implementación
// real en Windows, ver FirebasePushNotificationService en UI.Maui). El token en sí no se envía
// solo, siempre acompañado de un registro explícito contra el backend (ver IUserDeviceService),
// disparado desde el login -- este servicio solo expone el mecanismo nativo.
public interface IPushNotificationService
{
    // Null si la plataforma no soporta push (Windows) o si Firebase aún no ha entregado un token.
    Task<string?> GetTokenAsync();

    // Refresco de token (Plugin.Firebase.CloudMessaging.IFirebaseCloudMessaging.TokenChanged):
    // reenviar al backend cuando dispare, el token anterior deja de ser válido.
    event EventHandler<string>? TokenChanged;

    // Notificación recibida con la App en primer plano (o silenciosa) -- no dispara la notificación
    // nativa del sistema. Pensado para el banner ligero in-app, no para deep-link.
    event EventHandler<PushNotificationInfo>? NotificationReceived;

    // El usuario tocó la notificación (App en segundo plano o cerrada). PushNotificationInfo.Data
    // es donde debe viajar la clave de ruta para el deep-link (ver Routing.RegisterRoute en
    // AppShell).
    event EventHandler<PushNotificationInfo>? NotificationTapped;
}
