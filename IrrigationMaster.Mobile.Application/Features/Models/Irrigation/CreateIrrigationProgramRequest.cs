using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Espejo de CreateIrrigationProgramRequestDto del backend (IrrigationPrograms/Create). Nace
// siempre activo (el dominio lo fija así); no hay campo IsActive aquí, a diferencia de Update.
public class CreateIrrigationProgramRequest
{
    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("startTime")]
    public TimeSpan StartTime { get; init; }

    [JsonPropertyName("durationMinutes")]
    public int DurationMinutes { get; init; }

    [JsonPropertyName("daysOfWeek")]
    public string DaysOfWeek { get; init; } = string.Empty;

    [JsonPropertyName("hydraulicSectorId")]
    public Guid HydraulicSectorId { get; init; }

    [JsonPropertyName("seasonStartMonth")]
    public int? SeasonStartMonth { get; init; }

    [JsonPropertyName("seasonStartDay")]
    public int? SeasonStartDay { get; init; }

    [JsonPropertyName("seasonEndMonth")]
    public int? SeasonEndMonth { get; init; }

    [JsonPropertyName("seasonEndDay")]
    public int? SeasonEndDay { get; init; }
}
