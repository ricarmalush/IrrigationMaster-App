using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Espejo de Response<IEnumerable<WalkwayIrrigationStatusDto>> del backend (IrrigationTurns/status).
public class IrrigationStatusResponse
{
    [JsonPropertyName("data")]
    public List<WalkwayIrrigationStatusDto>? Data { get; init; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}
