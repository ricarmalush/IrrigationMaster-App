using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Espejo de Response<MyWalkwayIrrigationStatusDto> del backend (IrrigationTurns/my-walkway-status).
// Mismo patrón que IrrigationStatusResponse, pero con un único objeto en vez de una lista.
public class MyWalkwayIrrigationStatusResponse
{
    [JsonPropertyName("data")]
    public MyWalkwayIrrigationStatusDto? Data { get; init; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}
