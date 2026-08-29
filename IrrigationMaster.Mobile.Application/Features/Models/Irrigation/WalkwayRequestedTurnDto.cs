using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Espejo de WalkwayRequestedTurnDto del backend. Status llega SIN colapsar (puede ser
// "Requested" o "Pending", a diferencia de NeighborIrrigationStatusDto en la vista hermana
// "Estado de Riego") -- hoy la pantalla "Mi Riego" no lo muestra, solo nombre y hora.
public class WalkwayRequestedTurnDto
{
    [JsonPropertyName("turnId")]
    public Guid TurnId { get; init; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; init; }

    [JsonPropertyName("fullName")]
    public string FullName { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("scheduledStart")]
    public DateTime ScheduledStart { get; init; }

    [JsonPropertyName("scheduledEnd")]
    public DateTime ScheduledEnd { get; init; }
}
