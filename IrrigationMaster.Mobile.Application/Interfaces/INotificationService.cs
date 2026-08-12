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
}
