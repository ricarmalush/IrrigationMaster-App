using IrrigationMaster.Mobile.Application.Features.Models.Irrigation;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class FakeIrrigationService : IIrrigationService
{
    public List<WalkwayIrrigationStatusDto>? StatusToReturn { get; set; } = [];
    public UserActionResult StartTurnResult { get; set; } = new() { IsSuccess = true };
    public UserActionResult CompleteTurnResult { get; set; } = new() { IsSuccess = true };

    // Clave: HydraulicSectorId consultado -> true/false a devolver. Default true ("sin actividad
    // todavía") si el test no configura una entrada, mismo fallback que ApiService.
    public Dictionary<Guid, bool> IsIrrigationDayBySector { get; set; } = [];

    public Guid? LastStartTurnCall { get; private set; }
    public Guid? LastCompleteTurnCall { get; private set; }
    public Guid? LastIsIrrigationDayCall { get; private set; }

    public Task<List<WalkwayIrrigationStatusDto>?> GetIrrigationStatusAsync() => Task.FromResult(StatusToReturn);

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

    public Task<bool> IsIrrigationDayAsync(Guid hydraulicSectorId)
    {
        LastIsIrrigationDayCall = hydraulicSectorId;
        return Task.FromResult(IsIrrigationDayBySector.GetValueOrDefault(hydraulicSectorId, true));
    }
}
