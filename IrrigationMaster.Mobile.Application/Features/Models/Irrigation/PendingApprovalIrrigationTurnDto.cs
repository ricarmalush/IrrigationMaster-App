using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Espejo de PendingApprovalIrrigationTurnDto del backend. Id es el Id del propio IrrigationTurn --
// ApproveTurnAsync lo requiere para poder actuar sobre él.
public class PendingApprovalIrrigationTurnDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("requesterId")]
    public Guid RequesterId { get; init; }

    [JsonPropertyName("requesterFullName")]
    public string RequesterFullName { get; init; } = string.Empty;

    [JsonPropertyName("hydraulicSectorId")]
    public Guid HydraulicSectorId { get; init; }

    [JsonPropertyName("scheduledStart")]
    public DateTime ScheduledStart { get; init; }

    [JsonPropertyName("scheduledEnd")]
    public DateTime ScheduledEnd { get; init; }

    [JsonPropertyName("houseNumber")]
    public int? HouseNumber { get; init; }
}

// Espejo de PendingApprovalTurnsByWalkwayDto del backend: los turnos pendientes ya vienen
// agrupados por andador (solo los que tienen al menos uno) y ordenados dentro de cada grupo por
// prioridad -- HouseNumber descendente, ThenBy hora de solicitud.
public class PendingApprovalTurnsByWalkwayDto
{
    [JsonPropertyName("walkwayId")]
    public Guid WalkwayId { get; init; }

    [JsonPropertyName("walkwayCode")]
    public string WalkwayCode { get; init; } = string.Empty;

    [JsonPropertyName("turns")]
    public List<PendingApprovalIrrigationTurnDto> Turns { get; init; } = [];
}
