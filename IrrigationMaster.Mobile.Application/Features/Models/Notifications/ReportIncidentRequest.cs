namespace IrrigationMaster.Mobile.Application.Features.Models.Notifications;

// Espejo de ReportIncidentCommand del backend (Notifications/ReportIncident). El backend resuelve
// tanto al autor (ICurrentUser) como a los destinatarios (Presidentes de su misma organización)
// por su cuenta -- el body solo lleva la descripción del problema.
public class ReportIncidentRequest
{
    public string Message { get; set; } = string.Empty;
}
