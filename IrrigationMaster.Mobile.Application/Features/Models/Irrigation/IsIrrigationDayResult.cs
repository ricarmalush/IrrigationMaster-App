namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Resultado de IIrrigationService.IsIrrigationDayAsync: dos datos independientes. IsIrrigationDay
// se calcula SOLO a partir del Programa (día de semana + temporada) -- nunca de festivos. IsHoliday
// es un aviso puramente informativo aparte; un festivo NUNCA condiciona IsIrrigationDay ni bloquea
// la creación de turnos (el backend tampoco lo hace).
public class IsIrrigationDayResult
{
    public bool IsIrrigationDay { get; init; }
    public bool IsHoliday { get; init; }
}
