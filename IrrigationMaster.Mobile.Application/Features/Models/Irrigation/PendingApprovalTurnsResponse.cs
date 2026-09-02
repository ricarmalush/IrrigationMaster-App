using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Espejo de Response<IEnumerable<PendingApprovalTurnsByWalkwayDto>> del backend
// (IrrigationTurns/pending-approval) -- ya agrupado por andador.
public class PendingApprovalTurnsResponse
{
    [JsonPropertyName("data")]
    public List<PendingApprovalTurnsByWalkwayDto>? Data { get; init; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}
