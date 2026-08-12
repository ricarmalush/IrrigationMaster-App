namespace IrrigationMaster.Mobile.Application.Features.Models.Notifications;

// Espejo de SendNotificationCommand del backend (Notifications/Send). Audience/Type viajan como
// string (p. ej. "Organization"/"Info") porque el backend usa un JsonStringEnumConverter global
// para estos enums, no valores numéricos. El backend siempre resuelve la organización desde
// ICurrentUser -- no existe un campo OrganizationId en el contrato.
public class SendNotificationRequest
{
    public string Audience { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public Guid? TargetWalkwayId { get; set; }
}
