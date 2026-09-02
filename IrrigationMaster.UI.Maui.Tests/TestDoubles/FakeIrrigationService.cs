using IrrigationMaster.Mobile.Application.Features.Models.Irrigation;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class FakeIrrigationService : IIrrigationService
{
    public List<WalkwayIrrigationStatusDto>? StatusToReturn { get; set; } = [];
    public MyWalkwayIrrigationStatusDto? MyWalkwayStatusToReturn { get; set; }
    public List<IrrigationProgramDto>? ProgramsToReturn { get; set; } = [];
    public UserActionResult StartTurnResult { get; set; } = new() { IsSuccess = true };
    public UserActionResult CompleteTurnResult { get; set; } = new() { IsSuccess = true };
    public UserActionResult RequestTurnResult { get; set; } = new() { IsSuccess = true };
    public UserActionResult ApproveTurnResult { get; set; } = new() { IsSuccess = true };
    public List<PendingApprovalTurnsByWalkwayDto>? PendingApprovalTurnsToReturn { get; set; } = [];
    public UserActionResult CreateIrrigationProgramResult { get; set; } = new() { IsSuccess = true };
    public UserActionResult UpdateIrrigationProgramResult { get; set; } = new() { IsSuccess = true };

    public CreateIrrigationProgramRequest? LastCreateIrrigationProgramCall { get; private set; }
    public (Guid Id, UpdateIrrigationProgramRequest Request)? LastUpdateIrrigationProgramCall { get; private set; }

    // Clave: HydraulicSectorId consultado -> true/false a devolver. Default true ("sin actividad
    // todavía") si el test no configura una entrada, mismo fallback que ApiService.
    public Dictionary<Guid, bool> IsIrrigationDayBySector { get; set; } = [];

    public Guid? LastStartTurnCall { get; private set; }
    public Guid? LastCompleteTurnCall { get; private set; }
    public Guid? LastIsIrrigationDayCall { get; private set; }
    public Guid? LastApproveTurnCall { get; private set; }
    public (Guid HydraulicSectorId, Guid RequesterId, DateTime StartTime, DateTime EndTime)? LastRequestTurnCall { get; private set; }

    public Task<List<WalkwayIrrigationStatusDto>?> GetIrrigationStatusAsync() => Task.FromResult(StatusToReturn);

    public Task<MyWalkwayIrrigationStatusDto?> GetMyWalkwayStatusAsync() => Task.FromResult(MyWalkwayStatusToReturn);

    public Task<List<IrrigationProgramDto>?> GetIrrigationProgramsAsync() => Task.FromResult(ProgramsToReturn);

    public Task<UserActionResult> StartTurnAsync(Guid turnId)
    {
        LastStartTurnCall = turnId;
        return Task.FromResult(StartTurnResult);
    }

    public Task<UserActionResult> CompleteTurnAsync(Guid turnId)
    {
        LastCompleteTurnCall = turnId;
        return Task.FromResult(CompleteTurnResult);
    }

    public Task<UserActionResult> RequestTurnAsync(Guid hydraulicSectorId, Guid requesterId, DateTime startTime, DateTime endTime)
    {
        LastRequestTurnCall = (hydraulicSectorId, requesterId, startTime, endTime);
        return Task.FromResult(RequestTurnResult);
    }

    public Task<UserActionResult> ApproveTurnAsync(Guid turnId)
    {
        LastApproveTurnCall = turnId;
        return Task.FromResult(ApproveTurnResult);
    }

    public Task<List<PendingApprovalTurnsByWalkwayDto>?> GetPendingApprovalTurnsAsync() => Task.FromResult(PendingApprovalTurnsToReturn);

    public Task<UserActionResult> CreateIrrigationProgramAsync(CreateIrrigationProgramRequest request)
    {
        LastCreateIrrigationProgramCall = request;
        return Task.FromResult(CreateIrrigationProgramResult);
    }

    public Task<UserActionResult> UpdateIrrigationProgramAsync(Guid id, UpdateIrrigationProgramRequest request)
    {
        LastUpdateIrrigationProgramCall = (id, request);
        return Task.FromResult(UpdateIrrigationProgramResult);
    }

    public Task<bool> IsIrrigationDayAsync(Guid hydraulicSectorId)
    {
        LastIsIrrigationDayCall = hydraulicSectorId;
        return Task.FromResult(IsIrrigationDayBySector.GetValueOrDefault(hydraulicSectorId, true));
    }
}
