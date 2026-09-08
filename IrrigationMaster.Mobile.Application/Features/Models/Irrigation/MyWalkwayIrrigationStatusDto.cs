using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Espejo de MyWalkwayIrrigationStatusDto del backend (IrrigationTurns/my-walkway-status).
// WalkwayId/WalkwayCode son null cuando el llamador no tiene andador asignado (p. ej. un
// Presidente) -- estado válido de esta vista de solo lectura, no un error: ambas listas vienen
// vacías en ese caso, y la respuesta HTTP sigue siendo 200 OK.
public class MyWalkwayIrrigationStatusDto
{
    [JsonPropertyName("walkwayId")]
    public Guid? WalkwayId { get; init; }

    [JsonPropertyName("walkwayCode")]
    public string? WalkwayCode { get; init; }

    // Turnos Requested/Pending para mañana, ya ordenados por hora de inicio (ScheduledStart) --
    // el backend los devuelve así, no hace falta reordenar aquí.
    [JsonPropertyName("requestsTomorrow")]
    public List<WalkwayRequestedTurnDto> RequestsTomorrow { get; init; } = [];

    // Turnos Watering/Completed de hoy -- mismo vocabulario de estado que WalkwayIrrigationStatusDto
    // (la vista hermana "Estado de Riego"), reutilizable con IrrigationStatusViewModel.TranslateStatus.
    [JsonPropertyName("liveToday")]
    public List<NeighborIrrigationStatusDto> LiveToday { get; init; } = [];

    // Tramos horarios de los IrrigationProgram activos del sector que cubren hoy (puede haber 0, 1
    // o varios) -- antes el Vecino no tenía ninguna forma de ver la hora programada de riego de su
    // sector en esta pantalla, solo el estado de turnos ya solicitados.
    [JsonPropertyName("todaySchedule")]
    public List<TodayIrrigationScheduleDto> TodaySchedule { get; init; } = [];
}
