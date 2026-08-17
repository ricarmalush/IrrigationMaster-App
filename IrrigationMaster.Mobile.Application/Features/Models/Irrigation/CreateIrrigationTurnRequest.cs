using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Espejo de CreateIrrigationTurnRequestDto del backend (IrrigationTurns/Create). RequesterId
// siempre es el propio usuario logueado (CachedUserId) -- "Solicitar mi turno" no permite pedir un
// turno para otro vecino.
public class CreateIrrigationTurnRequest
{
    [JsonPropertyName("startTime")]
    public DateTime StartTime { get; init; }

    [JsonPropertyName("endTime")]
    public DateTime EndTime { get; init; }

    [JsonPropertyName("hydraulicSectorId")]
    public Guid HydraulicSectorId { get; init; }

    [JsonPropertyName("requesterId")]
    public Guid RequesterId { get; init; }

    [JsonPropertyName("priority")]
    public int Priority { get; init; } = 1;
}
