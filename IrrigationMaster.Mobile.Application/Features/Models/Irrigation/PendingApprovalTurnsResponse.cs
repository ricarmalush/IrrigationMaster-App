using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Espejo de Response<IEnumerable<PendingApprovalIrrigationTurnDto>> del backend
// (IrrigationTurns/pending-approval).
public class PendingApprovalTurnsResponse
{
    [JsonPropertyName("data")]
    public List<PendingApprovalIrrigationTurnDto>? Data { get; init; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}
