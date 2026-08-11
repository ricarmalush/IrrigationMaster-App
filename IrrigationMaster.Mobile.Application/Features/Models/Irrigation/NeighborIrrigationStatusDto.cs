using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Espejo de NeighborIrrigationStatusDto del backend. Status llega en inglés
// ("Watering"/"Waiting"/"Completed") -- la traducción a español vive en la capa de presentación
// (IrrigationStatusViewModel), no aquí. TurnId es el Id de IrrigationTurn (NO de UserId): es el
// que hace falta pasar a StartIrrigationTurnCommand/CompleteIrrigationTurnCommand.
public class NeighborIrrigationStatusDto
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; init; }

    [JsonPropertyName("turnId")]
    public Guid TurnId { get; init; }

    [JsonPropertyName("fullName")]
    public string FullName { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    [JsonPropertyName("scheduledStart")]
    public DateTime ScheduledStart { get; init; }

    [JsonPropertyName("scheduledEnd")]
    public DateTime ScheduledEnd { get; init; }
}
