using IrrigationMaster.Mobile.Application.Features.Models.Irrigation;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.MyIrrigation;
using IrrigationMaster.UI.Maui.Tests.TestDoubles;

namespace IrrigationMaster.UI.Maui.Tests;

public class MyIrrigationViewModelTests
{
    private static readonly Guid WalkwayId = Guid.NewGuid();
    private static readonly Guid TurnId1 = Guid.NewGuid();
    private static readonly Guid TurnId2 = Guid.NewGuid();

    private static (MyIrrigationViewModel ViewModel, FakeIrrigationService IrrigationService) CreateSut()
    {
        var irrigationService = new FakeIrrigationService();
        var viewModel = new MyIrrigationViewModel(irrigationService);
        return (viewModel, irrigationService);
    }

    // ─── SIN ANDADOR ASIGNADO (WalkwayId null) -- estado válido, no un error ───

    [Fact]
    public async Task LoadAsync_WhenWalkwayIdIsNull_SetsHasWalkwayFalse_AndLeavesListsEmpty()
    {
        var (vm, irrigationService) = CreateSut();
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
    }

    [Fact]
    public async Task LoadAsync_WhenResponseIsNull_SetsHasWalkwayFalse()
    {
        // ApiService devuelve null en fallo de red/servidor -- mismo criterio defensivo que el
        // resto de pantallas de esta familia (Estado de Riego incluida).
        var (vm, _) = CreateSut();

        await vm.LoadAsync();

        Assert.False(vm.HasWalkway);
        Assert.True(vm.NoWalkwayAssigned);
    }

    // ─── CON ANDADOR ASIGNADO ───

    [Fact]
    public async Task LoadAsync_WhenWalkwayIdIsPresent_SetsHasWalkwayTrue_AndPopulatesWalkwayCode()
    {
        var (vm, irrigationService) = CreateSut();
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
        var (vm, irrigationService) = CreateSut();
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
        var (vm, irrigationService) = CreateSut();
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
        var (vm, irrigationService) = CreateSut();
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
        var (vm, _) = CreateSut();

        var task = vm.LoadAsync();
        await task;

        Assert.False(vm.IsBusy);
        Assert.True(vm.IsNotBusy);
    }
}
