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
}
