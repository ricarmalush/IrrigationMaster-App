using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Espejo de Response<bool> del backend (IrrigationPrograms/IsIrrigationDay).
public class IsIrrigationDayResponse
{
    [JsonPropertyName("data")]
    public bool Data { get; init; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}
