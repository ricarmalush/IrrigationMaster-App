using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Espejo de TodayIrrigationScheduleDto del backend: un tramo horario de riego que aplica HOY para
// el sector del andador consultado (IrrigationProgram activo cuyo día de semana y temporada cubren
// hoy). Puede haber más de uno por sector el mismo día (p. ej. matutino y nocturno) -- Name viaja
// siempre, para distinguirlos cuando hay varios.
public class TodayIrrigationScheduleDto
{
    [JsonPropertyName("programId")]
    public Guid ProgramId { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("startTime")]
    public TimeSpan StartTime { get; init; }

    [JsonPropertyName("endTime")]
    public TimeSpan EndTime { get; init; }
}
