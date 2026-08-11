using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Espejo (parcial) de IrrigationProgramResponseDto del backend (IrrigationPrograms/pagination).
// Solo trae lo que necesita la línea de patrón de riego por andador en Estado de Riego -- Name/
// StartTime/DurationMinutes/Created no se usan aquí.
public class IrrigationProgramDto
{
    [JsonPropertyName("hydraulicSectorId")]
    public Guid HydraulicSectorId { get; init; }

    // CSV de enteros ISO-8601: Lunes=1 ... Domingo=7 (p. ej. "1,3,5"). Convención asumida por el
    // propio backend (ver GetIsIrrigationDayHandler), no forzada por ningún enum ni validación de
    // formato del lado servidor.
    [JsonPropertyName("daysOfWeek")]
    public string DaysOfWeek { get; init; } = string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; init; }

    // Todo-o-nada: o los 4 vienen con valor, o los 4 son null (sin restricción de temporada,
    // riega todo el año) -- validado así en el propio backend.
    [JsonPropertyName("seasonStartMonth")]
    public int? SeasonStartMonth { get; init; }

    [JsonPropertyName("seasonStartDay")]
    public int? SeasonStartDay { get; init; }

    [JsonPropertyName("seasonEndMonth")]
    public int? SeasonEndMonth { get; init; }

    [JsonPropertyName("seasonEndDay")]
    public int? SeasonEndDay { get; init; }
}
