using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Espejo de Response<IsIrrigationDayResponseDto> del backend (IrrigationPrograms/IsIrrigationDay).
public class IsIrrigationDayResponse
{
    [JsonPropertyName("data")]
    public IsIrrigationDayDataDto? Data { get; init; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}

// IsIrrigationDay e IsHoliday son deliberadamente independientes en el backend: un festivo NUNCA
// condiciona IsIrrigationDay ni bloquea la creación de turnos -- es solo un aviso informativo aparte.
public class IsIrrigationDayDataDto
{
    [JsonPropertyName("isIrrigationDay")]
    public bool IsIrrigationDay { get; init; }

    [JsonPropertyName("isHoliday")]
    public bool IsHoliday { get; init; }
}
