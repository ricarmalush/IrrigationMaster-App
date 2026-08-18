using IrrigationMaster.Mobile.Application.Common.Dtos;
using IrrigationMaster.Mobile.Application.Features.Models.Irrigation;
using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level3_Functional.IrrigationPrograms;
using IrrigationMaster.UI.Maui.Tests.TestDoubles;

namespace IrrigationMaster.UI.Maui.Tests;

public class IrrigationProgramsViewModelTests
{
    private static readonly Guid SectorAId = Guid.NewGuid();
    private static readonly Guid SectorBId = Guid.NewGuid();
    private static readonly Guid ProgramId = Guid.NewGuid();

    private static (IrrigationProgramsViewModel ViewModel, FakeIrrigationService IrrigationService, FakeStructureService StructureService, RecordingAlertService Alerts) CreateSut(
        List<IrrigationProgramDto>? programsToReturn = null,
        List<HydraulicSectorDto>? sectorsToReturn = null)
    {
        var irrigationService = new FakeIrrigationService { ProgramsToReturn = programsToReturn ?? [] };
        var structureService = new FakeStructureService { HydraulicSectorsToReturn = sectorsToReturn ?? [] };
        var alerts = new RecordingAlertService();

        var viewModel = new IrrigationProgramsViewModel(irrigationService, structureService, alerts);

        return (viewModel, irrigationService, structureService, alerts);
    }

    // ─── TEMPORADA "TODO O NADA" (TryParseSeason) ───

    [Fact]
    public void TryParseSeason_AllFourFieldsEmpty_IsValid_WithAllNulls()
    {
        var valid = IrrigationProgramsViewModel.TryParseSeason("", "", "", "",
            out var startMonth, out var startDay, out var endMonth, out var endDay);

        Assert.True(valid);
        Assert.Null(startMonth);
        Assert.Null(startDay);
        Assert.Null(endMonth);
        Assert.Null(endDay);
    }

    [Fact]
    public void TryParseSeason_AllFourFieldsFilled_IsValid_WithParsedValues()
    {
        var valid = IrrigationProgramsViewModel.TryParseSeason("3", "1", "11", "30",
            out var startMonth, out var startDay, out var endMonth, out var endDay);

        Assert.True(valid);
        Assert.Equal(3, startMonth);
        Assert.Equal(1, startDay);
        Assert.Equal(11, endMonth);
        Assert.Equal(30, endDay);
    }

    [Theory]
    [InlineData("3", "", "", "")]
    [InlineData("3", "1", "11", "")]
    [InlineData("", "1", "11", "30")]
    public void TryParseSeason_PartiallyFilled_IsInvalid(string startMonth, string startDay, string endMonth, string endDay)
    {
        var valid = IrrigationProgramsViewModel.TryParseSeason(startMonth, startDay, endMonth, endDay,
            out _, out _, out _, out _);

        Assert.False(valid);
    }

    [Fact]
    public void TryParseSeason_NonNumericValue_IsInvalid()
    {
        var valid = IrrigationProgramsViewModel.TryParseSeason("marzo", "1", "11", "30",
            out _, out _, out _, out _);

        Assert.False(valid);
    }

    // ─── CARGA: sectores + programas, con nombre de sector resuelto ───

    [Fact]
    public async Task LoadAsync_PopulatesSectorsAndPrograms_ResolvingSectorName()
    {
        var (vm, _, _, _) = CreateSut(
            programsToReturn: [new IrrigationProgramDto { Id = ProgramId, Name = "Riego Norte", HydraulicSectorId = SectorAId, DaysOfWeek = "1,3", IsActive = true }],
            sectorsToReturn: [new HydraulicSectorDto { Id = SectorAId, Name = "Sector Norte" }]);

        await vm.LoadAsync();

        Assert.Single(vm.HydraulicSectors);
        var program = Assert.Single(vm.Programs);
        Assert.Equal("Riego Norte", program.Name);
        Assert.Equal("Sector Norte", program.HydraulicSectorName);
    }

    [Fact]
    public async Task LoadAsync_ProgramWithUnknownSector_FallsBackToPlaceholderName()
    {
        var (vm, _, _, _) = CreateSut(
            programsToReturn: [new IrrigationProgramDto { Id = ProgramId, Name = "Riego Huérfano", HydraulicSectorId = SectorBId, DaysOfWeek = "1", IsActive = true }],
            sectorsToReturn: [new HydraulicSectorDto { Id = SectorAId, Name = "Sector Norte" }]);

        await vm.LoadAsync();

        Assert.Equal("Sector desconocido", vm.Programs.Single().HydraulicSectorName);
    }

    // ─── EDITAR: precarga el formulario, bloquea el cambio de sector ───

    [Fact]
    public async Task EditProgram_PopulatesFormFields_AndDisablesHydraulicSectorChange()
    {
        var (vm, _, _, _) = CreateSut(
            programsToReturn:
            [
                new IrrigationProgramDto
                {
                    Id = ProgramId,
                    Name = "Riego Norte",
                    StartTime = new TimeSpan(6, 30, 0),
                    DurationMinutes = 45,
                    DaysOfWeek = "1,3,5",
                    IsActive = true,
                    HydraulicSectorId = SectorAId,
                    SeasonStartMonth = 3,
                    SeasonStartDay = 1,
                    SeasonEndMonth = 11,
                    SeasonEndDay = 30
                }
            ],
            sectorsToReturn: [new HydraulicSectorDto { Id = SectorAId, Name = "Sector Norte" }]);
        await vm.LoadAsync();
        var listItem = vm.Programs.Single();

        vm.EditProgramCommand.Execute(listItem);

        Assert.True(vm.IsEditing);
        Assert.False(vm.CanChangeHydraulicSector);
        Assert.Equal("Riego Norte", vm.Name);
        Assert.Equal(new TimeSpan(6, 30, 0), vm.StartTime);
        Assert.Equal("45", vm.DurationMinutesText);
        Assert.Equal("1,3,5", vm.DaysOfWeekText);
        Assert.Equal(SectorAId, vm.SelectedHydraulicSector?.Id);
        Assert.Equal("3", vm.SeasonStartMonthText);
        Assert.Equal("Guardar cambios", vm.SaveButtonText);
    }

    [Fact]
    public void CancelEdit_ResetsFormToCreateMode()
    {
        var (vm, _, _, _) = CreateSut();
        vm.EditProgramCommand.Execute(new IrrigationProgramListItem { Id = ProgramId, Name = "Riego Norte", HydraulicSectorId = SectorAId, DaysOfWeek = "1" });

        vm.CancelEditCommand.Execute(null);

        Assert.False(vm.IsEditing);
        Assert.Equal(string.Empty, vm.Name);
        Assert.True(vm.CanChangeHydraulicSector);
    }

    // ─── GUARDAR: validación local ───

    [Fact]
    public async Task SaveAsync_MissingName_ShowsAttentionAlert_WithoutCallingService()
    {
        var (vm, irrigationService, _, alerts) = CreateSut();
        vm.DaysOfWeekText = "1,3";
        vm.DurationMinutesText = "30";

        await vm.SaveAsync();

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.AttentionTitle, alert.Title);
        Assert.Equal(AppStrings.MsgMissingIrrigationProgramData, alert.Message);
        Assert.Null(irrigationService.LastCreateIrrigationProgramCall);
    }

    [Fact]
    public async Task SaveAsync_NonNumericDuration_ShowsAttentionAlert_WithoutCallingService()
    {
        var (vm, irrigationService, _, alerts) = CreateSut();
        vm.Name = "Riego Norte";
        vm.DaysOfWeekText = "1,3";
        vm.DurationMinutesText = "no-numero";

        await vm.SaveAsync();

        Assert.Single(alerts.Calls);
        Assert.Null(irrigationService.LastCreateIrrigationProgramCall);
    }

    [Fact]
    public async Task SaveAsync_CreatingWithoutSelectedSector_ShowsAttentionAlert()
    {
        var (vm, irrigationService, _, alerts) = CreateSut();
        vm.Name = "Riego Norte";
        vm.DaysOfWeekText = "1,3";
        vm.DurationMinutesText = "30";
        // SelectedHydraulicSector se deja sin asignar a propósito.

        await vm.SaveAsync();

        Assert.Single(alerts.Calls);
        Assert.Null(irrigationService.LastCreateIrrigationProgramCall);
    }

    [Fact]
    public async Task SaveAsync_PartiallyFilledSeason_ShowsInvalidSeasonAlert_WithoutCallingService()
    {
        var (vm, irrigationService, structureService, alerts) = CreateSut(sectorsToReturn: [new HydraulicSectorDto { Id = SectorAId, Name = "Sector Norte" }]);
        await vm.LoadAsync();
        vm.Name = "Riego Norte";
        vm.DaysOfWeekText = "1,3";
        vm.DurationMinutesText = "30";
        vm.SelectedHydraulicSector = vm.HydraulicSectors.Single();
        vm.SeasonStartMonthText = "3"; // resto de campos de temporada quedan vacíos a propósito

        await vm.SaveAsync();

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.MsgInvalidSeasonData, alert.Message);
        Assert.Null(irrigationService.LastCreateIrrigationProgramCall);
    }

    // ─── GUARDAR: creación ───

    [Fact]
    public async Task SaveAsync_Creating_OnSuccess_CallsCreateWithSelectedSector_ShowsSuccessAlert_AndReloads()
    {
        var (vm, irrigationService, _, alerts) = CreateSut(sectorsToReturn: [new HydraulicSectorDto { Id = SectorAId, Name = "Sector Norte" }]);
        await vm.LoadAsync();
        vm.Name = "  Riego Norte  ";
        vm.DaysOfWeekText = "1,3";
        vm.DurationMinutesText = "30";
        vm.SelectedHydraulicSector = vm.HydraulicSectors.Single();

        await vm.SaveAsync();

        Assert.NotNull(irrigationService.LastCreateIrrigationProgramCall);
        Assert.Equal("Riego Norte", irrigationService.LastCreateIrrigationProgramCall!.Name);
        Assert.Equal(SectorAId, irrigationService.LastCreateIrrigationProgramCall!.HydraulicSectorId);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.SuccessTitle, alert.Title);
        Assert.Equal(AppStrings.IrrigationProgramCreatedSuccess, alert.Message);
        Assert.False(vm.IsEditing); // formulario reiniciado tras el éxito
        Assert.Equal(string.Empty, vm.Name);
    }

    [Fact]
    public async Task SaveAsync_Creating_WhenBackendRejects_ShowsExactBackendMessage_WithoutResettingForm()
    {
        var (vm, irrigationService, _, alerts) = CreateSut(sectorsToReturn: [new HydraulicSectorDto { Id = SectorAId, Name = "Sector Norte" }]);
        await vm.LoadAsync();
        vm.Name = "Riego Norte";
        vm.DaysOfWeekText = "1,3";
        vm.DurationMinutesText = "30";
        vm.SelectedHydraulicSector = vm.HydraulicSectors.Single();
        irrigationService.CreateIrrigationProgramResult = new UserActionResult { IsSuccess = false, Message = "Ya existe un programa con ese nombre." };

        await vm.SaveAsync();

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal("Ya existe un programa con ese nombre.", alert.Message);
        Assert.Equal("Riego Norte", vm.Name); // el formulario no se limpia si el backend rechaza
    }

    [Fact]
    public async Task SaveAsync_Creating_WhenBackendRejectsWithFieldErrors_JoinsThemIntoMessage()
    {
        var (vm, irrigationService, _, alerts) = CreateSut(sectorsToReturn: [new HydraulicSectorDto { Id = SectorAId, Name = "Sector Norte" }]);
        await vm.LoadAsync();
        vm.Name = "Riego Norte";
        vm.DaysOfWeekText = "1,3";
        vm.DurationMinutesText = "30";
        vm.SelectedHydraulicSector = vm.HydraulicSectors.Single();
        irrigationService.CreateIrrigationProgramResult = new UserActionResult
        {
            IsSuccess = false,
            Errors = [new ApiError { PropertyMessage = "DurationMinutes", ErrorMessage = "Debe ser mayor que cero." }]
        };

        await vm.SaveAsync();

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal("DurationMinutes: Debe ser mayor que cero.", alert.Message);
    }

    // ─── GUARDAR: edición ───

    [Fact]
    public async Task SaveAsync_Editing_OnSuccess_CallsUpdateWithEditingId_NotCreate()
    {
        var (vm, irrigationService, _, alerts) = CreateSut(
            programsToReturn: [new IrrigationProgramDto { Id = ProgramId, Name = "Riego Norte", HydraulicSectorId = SectorAId, DaysOfWeek = "1", DurationMinutes = 30, IsActive = true }],
            sectorsToReturn: [new HydraulicSectorDto { Id = SectorAId, Name = "Sector Norte" }]);
        await vm.LoadAsync();
        vm.EditProgramCommand.Execute(vm.Programs.Single());
        vm.Name = "Riego Norte Actualizado";

        await vm.SaveAsync();

        Assert.Null(irrigationService.LastCreateIrrigationProgramCall);
        Assert.NotNull(irrigationService.LastUpdateIrrigationProgramCall);
        Assert.Equal(ProgramId, irrigationService.LastUpdateIrrigationProgramCall!.Value.Id);
        Assert.Equal("Riego Norte Actualizado", irrigationService.LastUpdateIrrigationProgramCall!.Value.Request.Name);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.IrrigationProgramUpdatedSuccess, alert.Message);
    }

    [Fact]
    public async Task SaveAsync_Editing_DoesNotRequireSelectedSector()
    {
        // Al editar, el sector no es parte del formulario visible (CanChangeHydraulicSector es
        // false) -- SelectedHydraulicSector puede quedar null sin que eso bloquee el guardado.
        var (vm, irrigationService, _, alerts) = CreateSut(
            programsToReturn: [new IrrigationProgramDto { Id = ProgramId, Name = "Riego Norte", HydraulicSectorId = SectorAId, DaysOfWeek = "1", DurationMinutes = 30, IsActive = true }],
            sectorsToReturn: []);
        await vm.LoadAsync();
        vm.EditProgramCommand.Execute(vm.Programs.Single());

        await vm.SaveAsync();

        Assert.NotNull(irrigationService.LastUpdateIrrigationProgramCall);
        Assert.DoesNotContain(alerts.Calls, c => c.Title == AppStrings.AttentionTitle);
    }
}
