using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Espejo de UpdateIrrigationProgramRequestDto del backend (IrrigationPrograms/Update/{id}). Sin
// HydraulicSectorId a propósito: el backend no permite cambiar el sector de un programa existente,
// solo su configuración horaria/de recurrencia y su estado activo/inactivo.
public class UpdateIrrigationProgramRequest
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("startTime")]
    public TimeSpan StartTime { get; init; }

    [JsonPropertyName("durationMinutes")]
    public int DurationMinutes { get; init; }

    [JsonPropertyName("daysOfWeek")]
    public string DaysOfWeek { get; init; } = string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; init; }

    [JsonPropertyName("seasonStartMonth")]
    public int? SeasonStartMonth { get; init; }

    [JsonPropertyName("seasonStartDay")]
    public int? SeasonStartDay { get; init; }

    [JsonPropertyName("seasonEndMonth")]
    public int? SeasonEndMonth { get; init; }

    [JsonPropertyName("seasonEndDay")]
    public int? SeasonEndDay { get; init; }
}
