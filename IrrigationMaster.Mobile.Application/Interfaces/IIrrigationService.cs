using IrrigationMaster.Mobile.Application.Features.Models.Irrigation;
using IrrigationMaster.Mobile.Application.Features.Models.Users;

namespace IrrigationMaster.Mobile.Application.Interfaces;

// Estado de riego por andador (pantalla "Estado de Riego"), visible para cualquier autenticado de
// la organización sin permiso adicional -- el backend no exige ninguno para GetOrganizationStatus.
// El backend sí bloquea a nivel de negocio que un vecino normal actúe (Start/Complete) sobre el
// turno de otro; esta capa no duplica esa lógica, solo transporta la petición.
public interface IIrrigationService
{
    // Sin parámetro Date: siempre consulta "hoy" (el backend lo asume por defecto cuando se omite).
    Task<List<WalkwayIrrigationStatusDto>?> GetIrrigationStatusAsync();

    Task<UserActionResult> StartTurnAsync(Guid turnId);
    Task<UserActionResult> CompleteTurnAsync(Guid turnId);

    // Plantilla teórica (IrrigationProgram + HolidayCalendar), no confirma que exista un turno
    // real -- distingue "sin actividad todavía" (true) de "no hay riego programado hoy" (false)
    // para un andador sin ningún vecino en la respuesta de estado.
    Task<bool> IsIrrigationDayAsync(Guid hydraulicSectorId);
}
