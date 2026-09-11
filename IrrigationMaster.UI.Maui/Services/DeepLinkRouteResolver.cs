using IrrigationMaster.Mobile.Application.Features.Models.Devices;

namespace IrrigationMaster.UI.Maui.Services;

// Estático y testable a propósito (mismo motivo que AdminMenuPage.ComputeMenuVisibility): la
// lógica de "a qué ruta lleva esta notificación" no necesita ningún runtime de MAUI, así que se
// separa de PushNotificationCoordinator (que sí lo necesita, para navegar de verdad).
public static class DeepLinkRouteResolver
{
    // Clave acordada en el payload "data" del mensaje FCM -- el backend debe enviarla con la ruta
    // absoluta de Shell (p. ej. "//AdminMenuPage") cuando la notificación deba abrir una pantalla
    // concreta al tocarla. Sin esa clave (o vacía), no hay deep-link: se abre la App normal.
    public const string RouteDataKey = "route";

    public static string? Resolve(PushNotificationInfo notification) =>
        notification.Data.TryGetValue(RouteDataKey, out var route) && !string.IsNullOrWhiteSpace(route)
            ? route
            : null;
}
