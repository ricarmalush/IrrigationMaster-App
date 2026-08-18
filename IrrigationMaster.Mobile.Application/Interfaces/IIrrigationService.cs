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

    // Crea un IrrigationTurn en estado Requested para el propio solicitante -- nace sin aprobar
    // (Start() lo rechaza hasta que alguien con TURN_APPROVE/SUPERADMIN llame a ApproveTurnAsync).
    Task<UserActionResult> RequestTurnAsync(Guid hydraulicSectorId, Guid requesterId, DateTime startTime, DateTime endTime);

    // Requiere TURN_APPROVE o rol SUPERADMIN en el backend -- esta capa no duplica esa
    // comprobación, solo transporta la petición.
    Task<UserActionResult> ApproveTurnAsync(Guid turnId);

    // Turnos en Requested de la organización del llamante, para la pantalla de aprobación.
    Task<List<PendingApprovalIrrigationTurnDto>?> GetPendingApprovalTurnsAsync();

    // Plantilla teórica (IrrigationProgram + HolidayCalendar), no confirma que exista un turno
    // real -- distingue "sin actividad todavía" (true) de "no hay riego programado hoy" (false)
    // para un andador sin ningún vecino en la respuesta de estado.
    Task<bool> IsIrrigationDayAsync(Guid hydraulicSectorId);

    // Todos los IrrigationProgram de la organización del llamador (activos e inactivos -- el
    // filtrado por IsActive lo hace el consumidor). No hay filtro por HydraulicSectorId en el
    // backend, así que se trae todo de una vez y se cruza en memoria.
    Task<List<IrrigationProgramDto>?> GetIrrigationProgramsAsync();

    // Requiere SUPERADMIN o el permiso MANAGE_IRRIGATION_PROGRAMS (rol Coordinador de Riego) en
    // el backend -- esta capa no duplica esa comprobación, solo transporta la petición.
    Task<UserActionResult> CreateIrrigationProgramAsync(CreateIrrigationProgramRequest request);

    // Mismo gating que CreateIrrigationProgramAsync. El sector hidráulico no es editable aquí (ver
    // UpdateIrrigationProgramRequest).
    Task<UserActionResult> UpdateIrrigationProgramAsync(Guid id, UpdateIrrigationProgramRequest request);
}
