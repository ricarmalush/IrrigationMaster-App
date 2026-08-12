using IrrigationMaster.Mobile.Application.Features.Models.Notifications;
using IrrigationMaster.Mobile.Application.Features.Models.Users;

namespace IrrigationMaster.Mobile.Application.Interfaces;

// Notificaciones del propio usuario logueado (Notifications/Mine), visible para los 3 roles sin
// permiso adicional -- el backend solo exige estar autenticado. El backend ya ordena por Created
// descendente, no hace falta ordenar aquí.
public interface INotificationService
{
    Task<List<NotificationDto>?> GetMyNotificationsAsync();

    // El backend trata "marcar la notificación de otro usuario" igual que "no existe" (404/400
    // según el caso, siempre con IsSuccess=false) -- esta capa no duplica esa lógica, solo
    // transporta la petición y devuelve tal cual lo que el backend responda.
    Task<UserActionResult> MarkAsReadAsync(Guid notificationId);
    Task<UserActionResult> MarkAllAsReadAsync();

    // El backend resuelve destinatarios (Presidentes de la organización del autor) y permiso
    // (REPORT_INCIDENT, ya seedeado para VECINO) por su cuenta -- esta capa solo transporta la
    // descripción.
    Task<UserActionResult> ReportIncidentAsync(string message);

    // "Avisar a mi comunidad" (Presidente/SUPERADMIN, permiso SEND_NOTIFICATIONS). audience es
    // "Organization" o "Walkway" (el backend usa un JsonStringEnumConverter global, no valores
    // numéricos); targetWalkwayId solo aplica -- y es obligatorio -- cuando audience es "Walkway".
    // El backend siempre resuelve la organización desde el propio llamador, nunca desde el body.
    Task<SendNotificationResult> SendNotificationAsync(string audience, string message, Guid? targetWalkwayId);
}
