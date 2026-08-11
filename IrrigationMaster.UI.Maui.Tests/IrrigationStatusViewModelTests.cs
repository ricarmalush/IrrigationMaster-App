using IrrigationMaster.Mobile.Application.Features.Models.Irrigation;
using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.IrrigationStatus;
using IrrigationMaster.UI.Maui.Tests.TestDoubles;

namespace IrrigationMaster.UI.Maui.Tests;

public class IrrigationStatusViewModelTests
{
    private static readonly Guid MyUserId = Guid.NewGuid();
    private static readonly Guid OtherUserId = Guid.NewGuid();
    private static readonly Guid WalkwayAId = Guid.NewGuid();
    private static readonly Guid WalkwayBId = Guid.NewGuid();
    private static readonly Guid SectorId = Guid.NewGuid();
    private static readonly Guid TurnId = Guid.NewGuid();

    private static (IrrigationStatusViewModel ViewModel, FakeIrrigationService IrrigationService, FakeStructureService StructureService, RecordingAlertService Alerts) CreateSut(
        List<WalkwayIrrigationStatusDto>? statusToReturn = null)
    {
        var irrigationService = new FakeIrrigationService { StatusToReturn = statusToReturn ?? [] };
        var structureService = new FakeStructureService();
        var alerts = new RecordingAlertService();
        var session = new FakeCurrentSession { UserIdToReturn = MyUserId };

        var viewModel = new IrrigationStatusViewModel(irrigationService, structureService, alerts, session);

        return (viewModel, irrigationService, structureService, alerts);
    }

    // ─── TRADUCCIÓN DE ESTADO ───

    [Theory]
    [InlineData("Watering", "Regando")]
    [InlineData("Waiting", "Pendiente")]
    [InlineData("Completed", "Terminado")]
    public void TranslateStatus_MapsKnownBackendStatuses_ToSpanish(string rawStatus, string expectedDisplay)
    {
        Assert.Equal(expectedDisplay, IrrigationStatusViewModel.TranslateStatus(rawStatus));
    }

    [Fact]
    public void TranslateStatus_UnknownStatus_PassesThroughUnchanged()
    {
        // Defensivo ante estados del backend no contemplados aquí: mejor mostrar el valor crudo
        // que ocultar la fila o reventar.
        Assert.Equal("SomeFutureStatus", IrrigationStatusViewModel.TranslateStatus("SomeFutureStatus"));
    }

    // ─── PATRÓN DE RIEGO: traducción de DaysOfWeek/temporada a texto legible ───

    [Fact]
    public void BuildIrrigationPatternText_SingleDay_NoSeason_OmitsDatePart()
    {
        var program = new IrrigationProgramDto { DaysOfWeek = "1" };

        Assert.Equal("Este sector riega: Lunes", IrrigationStatusViewModel.BuildIrrigationPatternText(program));
    }

    [Fact]
    public void BuildIrrigationPatternText_TwoDays_JoinsWithY()
    {
        // Entrada en orden inverso (7,6): la salida debe seguir el orden natural de la semana
        // (Lunes...Domingo), no el orden del CSV.
        var program = new IrrigationProgramDto { DaysOfWeek = "7,6" };

        Assert.Equal("Este sector riega: Sábado y Domingo", IrrigationStatusViewModel.BuildIrrigationPatternText(program));
    }

    [Fact]
    public void BuildIrrigationPatternText_ThreeOrMoreDays_UsesCommasAndFinalY()
    {
        var program = new IrrigationProgramDto { DaysOfWeek = "1,3,5" };

        Assert.Equal("Este sector riega: Lunes, Miércoles y Viernes", IrrigationStatusViewModel.BuildIrrigationPatternText(program));
    }

    [Fact]
    public void BuildIrrigationPatternText_WithSeasonDeclared_AppendsMonthRange()
    {
        var program = new IrrigationProgramDto
        {
            DaysOfWeek = "6,7",
            SeasonStartMonth = 3,
            SeasonStartDay = 1,
            SeasonEndMonth = 11,
            SeasonEndDay = 30
        };

        Assert.Equal("Este sector riega: Sábado y Domingo, de marzo a noviembre", IrrigationStatusViewModel.BuildIrrigationPatternText(program));
    }

    [Fact]
    public void BuildIrrigationPatternText_AllFourSeasonFieldsNull_OmitsDatePart()
    {
        var program = new IrrigationProgramDto { DaysOfWeek = "6,7" };

        Assert.Equal("Este sector riega: Sábado y Domingo", IrrigationStatusViewModel.BuildIrrigationPatternText(program));
    }

    [Fact]
    public void BuildIrrigationPatternText_DuplicateAndUnparseableEntries_AreIgnored()
    {
        // El backend no valida el formato de DaysOfWeek (ver comentario en IrrigationProgramDto):
        // entradas repetidas, vacías o fuera de [1,7] no deben reventar ni duplicar el día.
        var program = new IrrigationProgramDto { DaysOfWeek = "1,1,abc,9,,1" };

        Assert.Equal("Este sector riega: Lunes", IrrigationStatusViewModel.BuildIrrigationPatternText(program));
    }

    [Fact]
    public void BuildIrrigationPatternText_NoValidDays_ReturnsNull()
    {
        var program = new IrrigationProgramDto { DaysOfWeek = "" };

        Assert.Null(IrrigationStatusViewModel.BuildIrrigationPatternText(program));
    }

    // ─── AGRUPAMIENTO Y TRADUCCIÓN AL CARGAR ───

    [Fact]
    public async Task LoadAsync_GroupsNeighborsByWalkway_WithTranslatedStatusAndOwnershipFlag()
    {
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new()
            {
                WalkwayId = WalkwayAId,
                WalkwayCode = "A-01",
                Neighbors =
                [
                    new NeighborIrrigationStatusDto { UserId = MyUserId, TurnId = TurnId, FullName = "Ana García", Status = "Waiting" },
                    new NeighborIrrigationStatusDto { UserId = OtherUserId, TurnId = Guid.NewGuid(), FullName = "Luis Pérez", Status = "Watering" }
                ]
            }
        };
        var (vm, _, _, _) = CreateSut(statusList);

        await vm.LoadAsync();

        var walkway = Assert.Single(vm.Walkways);
        Assert.Equal("A-01", walkway.WalkwayCode);
        Assert.True(walkway.HasNeighbors);
        Assert.Equal(2, walkway.Neighbors.Count);

        var mine = walkway.Neighbors.Single(n => n.UserId == MyUserId);
        Assert.True(mine.IsMine);
        Assert.Equal("Pendiente", mine.StatusDisplay);

        var other = walkway.Neighbors.Single(n => n.UserId == OtherUserId);
        Assert.False(other.IsMine);
        Assert.Equal("Regando", other.StatusDisplay);
    }

    [Fact]
    public async Task LoadAsync_WithMultipleWalkways_KeepsThemAsSeparateGroups()
    {
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [new NeighborIrrigationStatusDto { UserId = MyUserId, TurnId = TurnId, FullName = "Ana García", Status = "Waiting" }] },
            new() { WalkwayId = WalkwayBId, WalkwayCode = "A-02", Neighbors = [new NeighborIrrigationStatusDto { UserId = OtherUserId, TurnId = Guid.NewGuid(), FullName = "Luis Pérez", Status = "Completed" }] }
        };
        var (vm, _, _, _) = CreateSut(statusList);

        await vm.LoadAsync();

        Assert.Equal(2, vm.Walkways.Count);
        Assert.Contains(vm.Walkways, w => w.WalkwayCode == "A-01" && w.Neighbors.Single().FullName == "Ana García");
        Assert.Contains(vm.Walkways, w => w.WalkwayCode == "A-02" && w.Neighbors.Single().FullName == "Luis Pérez");
    }

    // ─── VISIBILIDAD CONDICIONAL DEL BOTÓN DE ACCIÓN ───

    [Fact]
    public async Task LoadAsync_MyOwnRow_Waiting_ShowsStartButton_NotCompleteButton()
    {
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [new NeighborIrrigationStatusDto { UserId = MyUserId, TurnId = TurnId, FullName = "Yo", Status = "Waiting" }] }
        };
        var (vm, _, _, _) = CreateSut(statusList);

        await vm.LoadAsync();

        var mine = vm.Walkways.Single().Neighbors.Single();
        Assert.True(mine.ShowStartButton);
        Assert.False(mine.ShowCompleteButton);
    }

    [Fact]
    public async Task LoadAsync_MyOwnRow_Watering_ShowsCompleteButton_NotStartButton()
    {
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [new NeighborIrrigationStatusDto { UserId = MyUserId, TurnId = TurnId, FullName = "Yo", Status = "Watering" }] }
        };
        var (vm, _, _, _) = CreateSut(statusList);

        await vm.LoadAsync();

        var mine = vm.Walkways.Single().Neighbors.Single();
        Assert.False(mine.ShowStartButton);
        Assert.True(mine.ShowCompleteButton);
    }

    [Theory]
    [InlineData("Waiting")]
    [InlineData("Watering")]
    [InlineData("Completed")]
    public async Task LoadAsync_OtherNeighborsRow_NeverShowsEitherButton_RegardlessOfStatus(string status)
    {
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [new NeighborIrrigationStatusDto { UserId = OtherUserId, TurnId = TurnId, FullName = "Otro Vecino", Status = status }] }
        };
        var (vm, _, _, _) = CreateSut(statusList);

        await vm.LoadAsync();

        var other = vm.Walkways.Single().Neighbors.Single();
        Assert.False(other.ShowStartButton);
        Assert.False(other.ShowCompleteButton);
    }

    [Fact]
    public async Task LoadAsync_MyOwnRow_Completed_ShowsNeitherButton()
    {
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [new NeighborIrrigationStatusDto { UserId = MyUserId, TurnId = TurnId, FullName = "Yo", Status = "Completed" }] }
        };
        var (vm, _, _, _) = CreateSut(statusList);

        await vm.LoadAsync();

        var mine = vm.Walkways.Single().Neighbors.Single();
        Assert.False(mine.ShowStartButton);
        Assert.False(mine.ShowCompleteButton);
    }

    // ─── ANDADOR SIN VECINOS: IsIrrigationDay ───

    [Fact]
    public async Task LoadAsync_EmptyWalkway_WhenIsIrrigationDayTrue_ShowsNoActivityYetMessage()
    {
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [] }
        };
        var (vm, irrigationService, structureService, _) = CreateSut(statusList);
        structureService.WalkwaysById[WalkwayAId] = new WalkwayDetailDto { Id = WalkwayAId, Code = "A-01", HydraulicSectorId = SectorId };
        irrigationService.IsIrrigationDayBySector[SectorId] = true;

        await vm.LoadAsync();

        var walkway = vm.Walkways.Single();
        Assert.False(walkway.HasNeighbors);
        Assert.Equal(IrrigationStatusViewModel.NoActivityYetMessage, walkway.EmptyStateMessage);
        Assert.Equal(SectorId, irrigationService.LastIsIrrigationDayCall);
    }

    [Fact]
    public async Task LoadAsync_EmptyWalkway_WhenIsIrrigationDayFalse_ShowsNoIrrigationScheduledMessage()
    {
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [] }
        };
        var (vm, irrigationService, structureService, _) = CreateSut(statusList);
        structureService.WalkwaysById[WalkwayAId] = new WalkwayDetailDto { Id = WalkwayAId, Code = "A-01", HydraulicSectorId = SectorId };
        irrigationService.IsIrrigationDayBySector[SectorId] = false;

        await vm.LoadAsync();

        var walkway = vm.Walkways.Single();
        Assert.Equal(IrrigationStatusViewModel.NoIrrigationScheduledMessage, walkway.EmptyStateMessage);
    }

    [Fact]
    public async Task LoadAsync_EmptyWalkway_WhenWalkwayDetailUnavailable_FallsBackToNoActivityYetMessage()
    {
        // GetWalkwayAsync devuelve null (p. ej. fallo de red): sin saber el sector no podemos
        // afirmar que no hay riego programado, así que se prefiere el mensaje menos alarmante.
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [] }
        };
        var (vm, irrigationService, _, _) = CreateSut(statusList);

        await vm.LoadAsync();

        var walkway = vm.Walkways.Single();
        Assert.Equal(IrrigationStatusViewModel.NoActivityYetMessage, walkway.EmptyStateMessage);
        Assert.Null(irrigationService.LastIsIrrigationDayCall);
    }

    [Fact]
    public async Task LoadAsync_WalkwayWithNeighbors_NeverCallsIsIrrigationDay()
    {
        // GetWalkwayAsync SÍ se llama (hace falta el HydraulicSectorId para el patrón de riego,
        // ver LoadAsync_WalkwayWithNeighbors_StillResolvesIrrigationPattern), pero IsIrrigationDay
        // es exclusivo de andadores sin vecinos: no tiene sentido resolverlo aquí.
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [new NeighborIrrigationStatusDto { UserId = MyUserId, TurnId = TurnId, FullName = "Yo", Status = "Waiting" }] }
        };
        var (vm, irrigationService, _, _) = CreateSut(statusList);

        await vm.LoadAsync();

        Assert.Null(irrigationService.LastIsIrrigationDayCall);
    }

    // ─── PATRÓN DE RIEGO: resolución del sector por andador y cruce con IrrigationProgram ───

    [Fact]
    public async Task LoadAsync_WalkwayWithActiveProgram_ShowsPatternLine()
    {
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [] }
        };
        var (vm, irrigationService, structureService, _) = CreateSut(statusList);
        structureService.WalkwaysById[WalkwayAId] = new WalkwayDetailDto { Id = WalkwayAId, Code = "A-01", HydraulicSectorId = SectorId };
        irrigationService.ProgramsToReturn =
        [
            new IrrigationProgramDto { HydraulicSectorId = SectorId, IsActive = true, DaysOfWeek = "6,7" }
        ];

        await vm.LoadAsync();

        var walkway = vm.Walkways.Single();
        Assert.True(walkway.ShowIrrigationPattern);
        Assert.Equal("Este sector riega: Sábado y Domingo", walkway.IrrigationPatternText);
    }

    [Fact]
    public async Task LoadAsync_WalkwayWithOnlyInactiveProgram_DoesNotShowPatternLine()
    {
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [] }
        };
        var (vm, irrigationService, structureService, _) = CreateSut(statusList);
        structureService.WalkwaysById[WalkwayAId] = new WalkwayDetailDto { Id = WalkwayAId, Code = "A-01", HydraulicSectorId = SectorId };
        irrigationService.ProgramsToReturn =
        [
            new IrrigationProgramDto { HydraulicSectorId = SectorId, IsActive = false, DaysOfWeek = "6,7" }
        ];

        await vm.LoadAsync();

        Assert.False(vm.Walkways.Single().ShowIrrigationPattern);
    }

    [Fact]
    public async Task LoadAsync_WalkwayWithNoMatchingProgram_DoesNotShowPatternLine()
    {
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [] }
        };
        var (vm, irrigationService, structureService, _) = CreateSut(statusList);
        structureService.WalkwaysById[WalkwayAId] = new WalkwayDetailDto { Id = WalkwayAId, Code = "A-01", HydraulicSectorId = SectorId };
        // Programa activo pero de OTRO sector -- no debe aplicar a este andador.
        irrigationService.ProgramsToReturn =
        [
            new IrrigationProgramDto { HydraulicSectorId = Guid.NewGuid(), IsActive = true, DaysOfWeek = "6,7" }
        ];

        await vm.LoadAsync();

        Assert.False(vm.Walkways.Single().ShowIrrigationPattern);
    }

    [Fact]
    public async Task LoadAsync_SectorWithTwoActivePrograms_ShowsOneLinePerProgram()
    {
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [] }
        };
        var (vm, irrigationService, structureService, _) = CreateSut(statusList);
        structureService.WalkwaysById[WalkwayAId] = new WalkwayDetailDto { Id = WalkwayAId, Code = "A-01", HydraulicSectorId = SectorId };
        irrigationService.ProgramsToReturn =
        [
            new IrrigationProgramDto { HydraulicSectorId = SectorId, IsActive = true, DaysOfWeek = "1,3,5" },
            new IrrigationProgramDto { HydraulicSectorId = SectorId, IsActive = true, DaysOfWeek = "6,7" }
        ];

        await vm.LoadAsync();

        var walkway = vm.Walkways.Single();
        Assert.Equal(2, walkway.IrrigationPatternLines.Count);
        Assert.Contains("Este sector riega: Lunes, Miércoles y Viernes", walkway.IrrigationPatternLines);
        Assert.Contains("Este sector riega: Sábado y Domingo", walkway.IrrigationPatternLines);
    }

    [Fact]
    public async Task LoadAsync_WalkwayWithNeighbors_StillResolvesIrrigationPattern()
    {
        // El patrón de riego es información del sector, independiente de si hoy hay actividad --
        // debe mostrarse tanto en andadores con vecinos como en andadores vacíos.
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [new NeighborIrrigationStatusDto { UserId = MyUserId, TurnId = TurnId, FullName = "Yo", Status = "Waiting" }] }
        };
        var (vm, irrigationService, structureService, _) = CreateSut(statusList);
        structureService.WalkwaysById[WalkwayAId] = new WalkwayDetailDto { Id = WalkwayAId, Code = "A-01", HydraulicSectorId = SectorId };
        irrigationService.ProgramsToReturn =
        [
            new IrrigationProgramDto { HydraulicSectorId = SectorId, IsActive = true, DaysOfWeek = "6,7" }
        ];

        await vm.LoadAsync();

        Assert.True(vm.Walkways.Single().ShowIrrigationPattern);
    }

    [Fact]
    public async Task LoadAsync_WhenWalkwayDetailUnavailable_DoesNotShowPatternLine_AndDoesNotCrash()
    {
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [] }
        };
        var (vm, irrigationService, _, _) = CreateSut(statusList);
        irrigationService.ProgramsToReturn =
        [
            new IrrigationProgramDto { HydraulicSectorId = SectorId, IsActive = true, DaysOfWeek = "6,7" }
        ];

        await vm.LoadAsync();

        Assert.False(vm.Walkways.Single().ShowIrrigationPattern);
    }

    // ─── EMPEZAR / TERMINAR TURNO ───

    [Fact]
    public async Task StartTurnAsync_OnSuccess_ShowsSuccessAlert_AndReloads()
    {
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [new NeighborIrrigationStatusDto { UserId = MyUserId, TurnId = TurnId, FullName = "Yo", Status = "Waiting" }] }
        };
        var (vm, irrigationService, _, alerts) = CreateSut(statusList);
        await vm.LoadAsync();
        var neighbor = vm.Walkways.Single().Neighbors.Single();

        await vm.StartTurnAsync(neighbor);

        Assert.Equal(TurnId, irrigationService.LastStartTurnCall);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.SuccessTitle, alert.Title);
        Assert.Equal(AppStrings.TurnStartedSuccess, alert.Message);
    }

    [Fact]
    public async Task StartTurnAsync_WhenBackendRejects_ShowsExactBackendMessage_WithoutReloading()
    {
        var (vm, irrigationService, _, alerts) = CreateSut();
        irrigationService.StartTurnResult = new UserActionResult
        {
            IsSuccess = false,
            Message = "El turno no está en estado Pendiente."
        };
        var neighbor = new NeighborStatusItem { UserId = MyUserId, TurnId = TurnId, FullName = "Yo", RawStatus = "Waiting", IsMine = true };

        await vm.StartTurnAsync(neighbor);

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal("El turno no está en estado Pendiente.", alert.Message);
    }

    [Fact]
    public async Task CompleteTurnAsync_OnSuccess_ShowsSuccessAlert_AndReloads()
    {
        var statusList = new List<WalkwayIrrigationStatusDto>
        {
            new() { WalkwayId = WalkwayAId, WalkwayCode = "A-01", Neighbors = [new NeighborIrrigationStatusDto { UserId = MyUserId, TurnId = TurnId, FullName = "Yo", Status = "Watering" }] }
        };
        var (vm, irrigationService, _, alerts) = CreateSut(statusList);
        await vm.LoadAsync();
        var neighbor = vm.Walkways.Single().Neighbors.Single();

        await vm.CompleteTurnAsync(neighbor);

        Assert.Equal(TurnId, irrigationService.LastCompleteTurnCall);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.SuccessTitle, alert.Title);
        Assert.Equal(AppStrings.TurnCompletedSuccess, alert.Message);
    }

    [Fact]
    public async Task CompleteTurnAsync_WhenBackendRejects_ShowsExactBackendMessage()
    {
        var (vm, irrigationService, _, alerts) = CreateSut();
        irrigationService.CompleteTurnResult = new UserActionResult
        {
            IsSuccess = false,
            Message = "La acción sobre 'Turno' no está permitida o el estado es inválido."
        };
        var neighbor = new NeighborStatusItem { UserId = MyUserId, TurnId = TurnId, FullName = "Yo", RawStatus = "Watering", IsMine = true };

        await vm.CompleteTurnAsync(neighbor);

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal("La acción sobre 'Turno' no está permitida o el estado es inválido.", alert.Message);
    }
}
