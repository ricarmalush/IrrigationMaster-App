using IrrigationMaster.Mobile.Application.Features.Models.Irrigation;
using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.MyIrrigation;
using IrrigationMaster.UI.Maui.Tests.TestDoubles;

namespace IrrigationMaster.UI.Maui.Tests;

public class MyIrrigationViewModelTests
{
    private static readonly Guid WalkwayId = Guid.NewGuid();
    private static readonly Guid HydraulicSectorId = Guid.NewGuid();
    private static readonly Guid TurnId1 = Guid.NewGuid();
    private static readonly Guid TurnId2 = Guid.NewGuid();
    private static readonly Guid MyUserId = Guid.NewGuid();

    private static (MyIrrigationViewModel ViewModel, FakeIrrigationService IrrigationService, FakeStructureService StructureService, FakeCurrentSession Session, RecordingAlertService AlertService) CreateSut(Guid? myUserId = null)
    {
        var irrigationService = new FakeIrrigationService();
        var structureService = new FakeStructureService();
        structureService.WalkwaysById[WalkwayId] = new WalkwayDetailDto { Id = WalkwayId, Code = "A-01", HydraulicSectorId = HydraulicSectorId };
        var session = new FakeCurrentSession { UserIdToReturn = myUserId ?? MyUserId };
        var alertService = new RecordingAlertService();

        var viewModel = new MyIrrigationViewModel(irrigationService, structureService, session, alertService);
        return (viewModel, irrigationService, structureService, session, alertService);
    }

    // ─── SIN ANDADOR ASIGNADO (WalkwayId null) -- estado válido, no un error ───

    [Fact]
    public async Task LoadAsync_WhenWalkwayIdIsNull_SetsHasWalkwayFalse_AndLeavesListsEmpty()
    {
        var (vm, irrigationService, structureService, _, _) = CreateSut();
        irrigationService.MyWalkwayStatusToReturn = new MyWalkwayIrrigationStatusDto
        {
            WalkwayId = null,
            WalkwayCode = null,
            RequestsTomorrow = [],
            LiveToday = []
        };

        await vm.LoadAsync();

        Assert.False(vm.HasWalkway);
        Assert.True(vm.NoWalkwayAssigned);
        Assert.Empty(vm.RequestsTomorrow);
        Assert.Empty(vm.LiveToday);
        Assert.False(vm.CanRequestTurn);
        Assert.Null(structureService.LastGetWalkwayCall);
    }

    [Fact]
    public async Task LoadAsync_WhenResponseIsNull_SetsHasWalkwayFalse()
    {
        // ApiService devuelve null en fallo de red/servidor -- mismo criterio defensivo que el
        // resto de pantallas de esta familia (Estado de Riego incluida).
        var (vm, _, _, _, _) = CreateSut();

        await vm.LoadAsync();

        Assert.False(vm.HasWalkway);
        Assert.True(vm.NoWalkwayAssigned);
    }

    // ─── CON ANDADOR ASIGNADO ───

    [Fact]
    public async Task LoadAsync_WhenWalkwayIdIsPresent_SetsHasWalkwayTrue_AndPopulatesWalkwayCode()
    {
        var (vm, irrigationService, _, _, _) = CreateSut();
        irrigationService.MyWalkwayStatusToReturn = new MyWalkwayIrrigationStatusDto
        {
            WalkwayId = WalkwayId,
            WalkwayCode = "A-01",
            RequestsTomorrow = [],
            LiveToday = []
        };

        await vm.LoadAsync();

        Assert.True(vm.HasWalkway);
        Assert.False(vm.NoWalkwayAssigned);
        Assert.Equal("A-01", vm.WalkwayCode);
    }

    [Fact]
    public async Task LoadAsync_PopulatesRequestsTomorrow_PreservingBackendOrder()
    {
        // El backend ya devuelve RequestsTomorrow ordenado por ScheduledStart -- el ViewModel no
        // debe reordenar, solo mapear tal cual.
        var (vm, irrigationService, _, _, _) = CreateSut();
        var early = new DateTime(2026, 3, 10, 7, 0, 0, DateTimeKind.Utc);
        var late = new DateTime(2026, 3, 10, 18, 30, 0, DateTimeKind.Utc);
        irrigationService.MyWalkwayStatusToReturn = new MyWalkwayIrrigationStatusDto
        {
            WalkwayId = WalkwayId,
            WalkwayCode = "A-01",
            RequestsTomorrow =
            [
                new WalkwayRequestedTurnDto { TurnId = TurnId1, FullName = "Ana García", Status = "Requested", ScheduledStart = early },
                new WalkwayRequestedTurnDto { TurnId = TurnId2, FullName = "Luis Pérez", Status = "Pending", ScheduledStart = late }
            ],
            LiveToday = []
        };

        await vm.LoadAsync();

        Assert.Equal(2, vm.RequestsTomorrow.Count);
        Assert.Equal("Ana García", vm.RequestsTomorrow[0].FullName);
        Assert.Equal("07:00", vm.RequestsTomorrow[0].ScheduledStartDisplay);
        Assert.Equal("Luis Pérez", vm.RequestsTomorrow[1].FullName);
        Assert.Equal("18:30", vm.RequestsTomorrow[1].ScheduledStartDisplay);
    }

    [Fact]
    public async Task LoadAsync_PopulatesLiveToday_WithTranslatedStatus()
    {
        var (vm, irrigationService, _, _, _) = CreateSut();
        irrigationService.MyWalkwayStatusToReturn = new MyWalkwayIrrigationStatusDto
        {
            WalkwayId = WalkwayId,
            WalkwayCode = "A-01",
            RequestsTomorrow = [],
            LiveToday =
            [
                new NeighborIrrigationStatusDto { UserId = Guid.NewGuid(), TurnId = TurnId1, FullName = "Ana García", Status = "Watering" },
                new NeighborIrrigationStatusDto { UserId = Guid.NewGuid(), TurnId = TurnId2, FullName = "Luis Pérez", Status = "Completed" }
            ]
        };

        await vm.LoadAsync();

        Assert.Equal(2, vm.LiveToday.Count);
        var watering = vm.LiveToday.Single(t => t.FullName == "Ana García");
        Assert.Equal("Regando", watering.StatusDisplay);
        Assert.False(watering.IsCompleted);

        var completed = vm.LiveToday.Single(t => t.FullName == "Luis Pérez");
        Assert.Equal("Terminado", completed.StatusDisplay);
        Assert.True(completed.IsCompleted);
    }

    [Fact]
    public async Task LoadAsync_ClearsPreviousResults_BeforeRepopulating()
    {
        // Una segunda carga (p. ej. al volver a OnAppearing) no debe acumular filas de la anterior.
        var (vm, irrigationService, _, _, _) = CreateSut();
        irrigationService.MyWalkwayStatusToReturn = new MyWalkwayIrrigationStatusDto
        {
            WalkwayId = WalkwayId,
            WalkwayCode = "A-01",
            RequestsTomorrow = [new WalkwayRequestedTurnDto { TurnId = TurnId1, FullName = "Ana García", Status = "Requested", ScheduledStart = DateTime.UtcNow }],
            LiveToday = [new NeighborIrrigationStatusDto { UserId = Guid.NewGuid(), TurnId = TurnId2, FullName = "Luis Pérez", Status = "Watering" }]
        };
        await vm.LoadAsync();

        irrigationService.MyWalkwayStatusToReturn = new MyWalkwayIrrigationStatusDto
        {
            WalkwayId = WalkwayId,
            WalkwayCode = "A-01",
            RequestsTomorrow = [],
            LiveToday = []
        };
        await vm.LoadAsync();

        Assert.Empty(vm.RequestsTomorrow);
        Assert.Empty(vm.LiveToday);
    }

    [Fact]
    public async Task LoadAsync_TogglesIsBusy_DuringLoad()
    {
        var (vm, _, _, _, _) = CreateSut();

        var task = vm.LoadAsync();
        await task;

        Assert.False(vm.IsBusy);
        Assert.True(vm.IsNotBusy);
    }

    // ─── "Solicitar mi turno": vive aquí también desde que "Estado de Riego" para Vecino pasó a
    // apuntar a esta pantalla -- mismo criterio que CanRequestTurn en WalkwayStatusItem, la vista
    // hermana donde vivía originalmente esta acción ───

    [Fact]
    public async Task LoadAsync_ResolvesHydraulicSectorId_FromTheWalkway()
    {
        var (vm, irrigationService, structureService, _, _) = CreateSut();
        irrigationService.MyWalkwayStatusToReturn = new MyWalkwayIrrigationStatusDto
        {
            WalkwayId = WalkwayId,
            WalkwayCode = "A-01",
            RequestsTomorrow = [],
            LiveToday = []
        };

        await vm.LoadAsync();

        Assert.Equal(WalkwayId, structureService.LastGetWalkwayCall);
        Assert.True(vm.CanRequestTurn);
    }

    [Fact]
    public async Task CanRequestTurn_IsFalse_WhenCallerAlreadyHasATurnToday_RegardlessOfStatus()
    {
        var (vm, irrigationService, _, _, _) = CreateSut();
        irrigationService.MyWalkwayStatusToReturn = new MyWalkwayIrrigationStatusDto
        {
            WalkwayId = WalkwayId,
            WalkwayCode = "A-01",
            RequestsTomorrow = [],
            LiveToday = [new NeighborIrrigationStatusDto { UserId = MyUserId, TurnId = TurnId1, FullName = "Yo", Status = "Completed" }]
        };

        await vm.LoadAsync();

        Assert.False(vm.CanRequestTurn);
    }

    [Fact]
    public async Task CanRequestTurn_IsTrue_WhenTodaysTurnsBelongToOtherNeighbors()
    {
        var (vm, irrigationService, _, _, _) = CreateSut();
        irrigationService.MyWalkwayStatusToReturn = new MyWalkwayIrrigationStatusDto
        {
            WalkwayId = WalkwayId,
            WalkwayCode = "A-01",
            RequestsTomorrow = [],
            LiveToday = [new NeighborIrrigationStatusDto { UserId = Guid.NewGuid(), TurnId = TurnId1, FullName = "Otro Vecino", Status = "Watering" }]
        };

        await vm.LoadAsync();

        Assert.True(vm.CanRequestTurn);
    }

    [Fact]
    public async Task RequestTurnAsync_WhenCanRequestTurnIsFalse_DoesNothing()
    {
        var (vm, irrigationService, _, _, _) = CreateSut();
        irrigationService.MyWalkwayStatusToReturn = new MyWalkwayIrrigationStatusDto { WalkwayId = null, WalkwayCode = null, RequestsTomorrow = [], LiveToday = [] };
        await vm.LoadAsync();

        await vm.RequestTurnAsync();

        Assert.Null(irrigationService.LastRequestTurnCall);
    }

    [Fact]
    public async Task RequestTurnAsync_OnSuccess_RequestsA2hTurn_ShowsSuccessAlert_AndReloads()
    {
        var (vm, irrigationService, _, _, alertService) = CreateSut();
        irrigationService.MyWalkwayStatusToReturn = new MyWalkwayIrrigationStatusDto { WalkwayId = WalkwayId, WalkwayCode = "A-01", RequestsTomorrow = [], LiveToday = [] };
        await vm.LoadAsync();
        irrigationService.RequestTurnResult = new UserActionResult { IsSuccess = true };

        await vm.RequestTurnAsync();

        Assert.NotNull(irrigationService.LastRequestTurnCall);
        var call = irrigationService.LastRequestTurnCall!.Value;
        Assert.Equal(HydraulicSectorId, call.HydraulicSectorId);
        Assert.Equal(MyUserId, call.RequesterId);
        Assert.Equal(MyIrrigationViewModel.DefaultTurnDurationHours, (call.EndTime - call.StartTime).TotalHours);
        Assert.Equal(AppStrings.SuccessTitle, alertService.Calls[0].Title);
    }

    [Fact]
    public async Task RequestTurnAsync_OnBackendFailure_ShowsErrorAlert_WithBackendMessage()
    {
        var (vm, irrigationService, _, _, alertService) = CreateSut();
        irrigationService.MyWalkwayStatusToReturn = new MyWalkwayIrrigationStatusDto { WalkwayId = WalkwayId, WalkwayCode = "A-01", RequestsTomorrow = [], LiveToday = [] };
        await vm.LoadAsync();
        irrigationService.RequestTurnResult = new UserActionResult { IsSuccess = false, Message = "La fecha de inicio debe ser futura." };

        await vm.RequestTurnAsync();

        Assert.Equal(AppStrings.ErrorTitle, alertService.Calls[0].Title);
        Assert.Equal("La fecha de inicio debe ser futura.", alertService.Calls[0].Message);
    }
}
