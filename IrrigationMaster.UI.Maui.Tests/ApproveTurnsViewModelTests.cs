using IrrigationMaster.Mobile.Application.Features.Models.Irrigation;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.ApproveTurns;
using IrrigationMaster.UI.Maui.Tests.TestDoubles;

namespace IrrigationMaster.UI.Maui.Tests;

public class ApproveTurnsViewModelTests
{
    private static readonly Guid WalkwayId1 = Guid.NewGuid();
    private static readonly Guid WalkwayId2 = Guid.NewGuid();
    private static readonly Guid TurnId1 = Guid.NewGuid();
    private static readonly Guid TurnId2 = Guid.NewGuid();

    private static (ApproveTurnsViewModel ViewModel, FakeIrrigationService IrrigationService, RecordingAlertService AlertService) CreateSut()
    {
        var irrigationService = new FakeIrrigationService();
        var alertService = new RecordingAlertService();
        var viewModel = new ApproveTurnsViewModel(irrigationService, alertService);
        return (viewModel, irrigationService, alertService);
    }

    [Fact]
    public async Task LoadAsync_PopulatesGroups_AlreadyGroupedByWalkway_PreservingBackendOrder()
    {
        // El backend ya devuelve los grupos y, dentro de cada uno, los turnos ordenados por
        // prioridad (HouseNumber descendente, ThenBy hora de solicitud) -- el ViewModel no debe
        // reordenar nada, solo mapear tal cual.
        var (vm, irrigationService, _) = CreateSut();
        irrigationService.PendingApprovalTurnsToReturn =
        [
            new PendingApprovalTurnsByWalkwayDto
            {
                WalkwayId = WalkwayId1,
                WalkwayCode = "A-01",
                Turns =
                [
                    new PendingApprovalIrrigationTurnDto { Id = TurnId1, RequesterFullName = "Casa 12", HouseNumber = 12 },
                    new PendingApprovalIrrigationTurnDto { Id = TurnId2, RequesterFullName = "Casa 5", HouseNumber = 5 }
                ]
            },
            new PendingApprovalTurnsByWalkwayDto
            {
                WalkwayId = WalkwayId2,
                WalkwayCode = "B-02",
                Turns = [new PendingApprovalIrrigationTurnDto { Id = Guid.NewGuid(), RequesterFullName = "Casa 3", HouseNumber = 3 }]
            }
        ];

        await vm.LoadAsync();

        Assert.Equal(2, vm.Groups.Count);

        var groupA = vm.Groups[0];
        Assert.Equal("A-01", groupA.WalkwayCode);
        Assert.Equal(2, groupA.Turns.Count);
        Assert.Equal("Casa 12", groupA.Turns[0].RequesterFullName);
        Assert.Equal("12", groupA.Turns[0].HouseNumberDisplay);
        Assert.Equal("Casa 5", groupA.Turns[1].RequesterFullName);

        var groupB = vm.Groups[1];
        Assert.Equal("B-02", groupB.WalkwayCode);
        Assert.Single(groupB.Turns);
    }

    [Fact]
    public void HouseNumberDisplay_WhenNull_ShowsPlaceholder()
    {
        var turn = new PendingTurnItem { HouseNumber = null };

        Assert.Equal("—", turn.HouseNumberDisplay);
    }

    [Fact]
    public async Task LoadAsync_WhenResponseIsNull_LeavesGroupsEmpty()
    {
        var (vm, _, _) = CreateSut();

        await vm.LoadAsync();

        Assert.Empty(vm.Groups);
    }

    [Fact]
    public async Task LoadAsync_ClearsPreviousGroups_BeforeRepopulating()
    {
        var (vm, irrigationService, _) = CreateSut();
        irrigationService.PendingApprovalTurnsToReturn =
        [
            new PendingApprovalTurnsByWalkwayDto { WalkwayId = WalkwayId1, WalkwayCode = "A-01", Turns = [new PendingApprovalIrrigationTurnDto { Id = TurnId1 }] }
        ];
        await vm.LoadAsync();

        irrigationService.PendingApprovalTurnsToReturn = [];
        await vm.LoadAsync();

        Assert.Empty(vm.Groups);
    }

    [Fact]
    public async Task ApproveAsync_OnSuccess_ShowsSuccessAlert_AndReloads()
    {
        var (vm, irrigationService, alertService) = CreateSut();
        irrigationService.ApproveTurnResult = new UserActionResult { IsSuccess = true };
        var turn = new PendingTurnItem { Id = TurnId1, RequesterFullName = "Casa 12" };

        await vm.ApproveAsync(turn);

        Assert.Equal(TurnId1, irrigationService.LastApproveTurnCall);
        Assert.Equal(AppStrings.SuccessTitle, alertService.Calls[0].Title);
    }

    [Fact]
    public async Task ApproveAsync_OnFailure_ShowsErrorAlert_WithBackendMessage()
    {
        var (vm, irrigationService, alertService) = CreateSut();
        irrigationService.ApproveTurnResult = new UserActionResult { IsSuccess = false, Message = "No tienes permiso para realizar esta acción." };
        var turn = new PendingTurnItem { Id = TurnId1 };

        await vm.ApproveAsync(turn);

        Assert.Equal(AppStrings.ErrorTitle, alertService.Calls[0].Title);
        Assert.Equal("No tienes permiso para realizar esta acción.", alertService.Calls[0].Message);
    }

    [Fact]
    public async Task ApproveAsync_WithNullTurn_DoesNothing()
    {
        var (vm, irrigationService, _) = CreateSut();

        await vm.ApproveAsync(null);

        Assert.Null(irrigationService.LastApproveTurnCall);
    }
}
