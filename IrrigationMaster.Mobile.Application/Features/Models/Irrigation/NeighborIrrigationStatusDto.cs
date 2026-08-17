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

    // false mientras el turno sigue en Requested (nadie con TURN_APPROVE/SUPERADMIN lo ha
    // aprobado todavía) -- el propio solicitante lo necesita para saber si ya puede pulsar
    // "Empezar mi turno" o si sigue esperando aprobación, algo que Status por sí solo no distingue
    // (Requested y Pending colapsan ambos a "Waiting").
    [JsonPropertyName("isApproved")]
    public bool IsApproved { get; init; }
}
