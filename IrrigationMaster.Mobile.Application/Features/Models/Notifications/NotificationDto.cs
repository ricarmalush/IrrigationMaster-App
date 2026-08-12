using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Notifications;

// Espejo (parcial) de NotificationResponseDto del backend (Notifications/Mine). Solo trae lo que
// necesita la pantalla de Notificaciones -- Type/ReadAt/UserId/OrganizationId/CreatedBy no se
// usan aquí.
public class NotificationDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("title")]
    public string Title { get; init; } = string.Empty;

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("isRead")]
    public bool IsRead { get; init; }

    [JsonPropertyName("created")]
    public DateTime Created { get; init; }
}
