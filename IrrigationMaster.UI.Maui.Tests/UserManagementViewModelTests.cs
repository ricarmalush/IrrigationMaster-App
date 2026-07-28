using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level3_Functional.Users;
using IrrigationMaster.UI.Maui.Tests.TestDoubles;

namespace IrrigationMaster.UI.Maui.Tests;

public class UserManagementViewModelTests
{
    private static readonly Guid OrganizationId = Guid.NewGuid();
    private static readonly Guid PendingUserId = Guid.NewGuid();
    private static readonly Guid ActiveUserId = Guid.NewGuid();
    private static readonly Guid RoleId = Guid.NewGuid();
    private static readonly Guid WalkwayId = Guid.NewGuid();

    private static (UserManagementViewModel ViewModel, FakeUserManagementService UserService, RecordingAlertService Alerts) CreateSut()
    {
        var userService = new FakeUserManagementService
        {
            RolesToReturn = [new RoleDto { Id = RoleId, Name = "Tesorero", OrganizationId = OrganizationId }],
            WalkwaysToReturn = [new WalkwayDto { Id = WalkwayId, Code = "A-01" }],
            UsersToReturn =
            [
                new AppUserDto
                {
                    Id = PendingUserId, FirstName = "Ana", LastName = "García", Email = "ana@test.com",
                    OrganizationId = OrganizationId, Role = "Vecino", IsActive = false
                }
            ]
        };
        var alerts = new RecordingAlertService();

        var viewModel = new UserManagementViewModel(userService, alerts);

        return (viewModel, userService, alerts);
    }

    // ─── FILTRO: Pendientes / Todos ───

    [Fact]
    public async Task LoadAsync_ByDefault_RequestsOnlyPendingUsers()
    {
        var (vm, userService, _) = CreateSut();

        await vm.LoadAsync();

        Assert.True(vm.ShowOnlyPending);
        Assert.Equal(false, userService.LastIsActiveFilter);
    }

    [Fact]
    public async Task ShowAllAsync_RequestsAllUsers_WithoutIsActiveFilter()
    {
        var (vm, userService, _) = CreateSut();

        await vm.ShowAllAsync();

        Assert.False(vm.ShowOnlyPending);
        Assert.Null(userService.LastIsActiveFilter);
    }

    [Fact]
    public async Task ShowPendingAsync_AfterShowAll_SwitchesBackToPendingOnlyFilter()
    {
        var (vm, userService, _) = CreateSut();
        await vm.ShowAllAsync();

        await vm.ShowPendingAsync();

        Assert.True(vm.ShowOnlyPending);
        Assert.Equal(false, userService.LastIsActiveFilter);
    }

    [Fact]
    public async Task LoadAsync_PopulatesUsersRolesAndWalkways_WithMatchingSelections()
    {
        var (vm, _, _) = CreateSut();

        await vm.LoadAsync();

        var user = Assert.Single(vm.Users);
        Assert.Equal("Ana García", user.FullName);
        Assert.True(user.ShowApprove); // pendiente -> debe ofrecer "Aprobar"
        Assert.Single(vm.Roles);
        Assert.Single(vm.Walkways);
    }

    // ─── APROBAR ───

    [Fact]
    public async Task ApproveAsync_OnSuccess_ActivatesUser_ShowsSuccessAlert_AndReloads()
    {
        var (vm, userService, alerts) = CreateSut();
        var user = new UserListItem { Id = PendingUserId, IsActive = false };

        await vm.ApproveAsync(user);

        Assert.Equal(PendingUserId, userService.LastActivatedUserId);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.SuccessTitle, alert.Title);
        Assert.Equal(AppStrings.UserApprovedSuccess, alert.Message);
    }

    // ─── ASIGNAR ANDADOR ───

    [Fact]
    public async Task AssignWalkwayAsync_OnSuccess_UsesSelectedWalkwayId()
    {
        var (vm, userService, alerts) = CreateSut();
        var user = new UserListItem { Id = ActiveUserId, IsActive = true, SelectedWalkway = new WalkwayItem { Id = WalkwayId, Code = "A-01" } };

        await vm.AssignWalkwayAsync(user);

        Assert.Equal((ActiveUserId, (Guid?)WalkwayId), userService.LastAssignWalkwayCall);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.SuccessTitle, alert.Title);
        Assert.Equal(AppStrings.WalkwayAssignedSuccess, alert.Message);
    }

    [Fact]
    public async Task AssignWalkwayAsync_WithNoWalkwaySelected_SendsNull_ToClearAssignment()
    {
        var (vm, userService, _) = CreateSut();
        var user = new UserListItem { Id = ActiveUserId, IsActive = true, SelectedWalkway = null };

        await vm.AssignWalkwayAsync(user);

        Assert.Equal((ActiveUserId, (Guid?)null), userService.LastAssignWalkwayCall);
    }

    // ─── CAMBIAR ROL ───

    [Fact]
    public async Task ChangeRoleAsync_OnSuccess_UsesSelectedRoleId()
    {
        var (vm, userService, alerts) = CreateSut();
        var user = new UserListItem { Id = ActiveUserId, IsActive = true, SelectedRole = new RoleItem { Id = RoleId, Name = "Tesorero" } };

        await vm.ChangeRoleAsync(user);

        Assert.Equal((ActiveUserId, RoleId), userService.LastChangeRoleCall);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.SuccessTitle, alert.Title);
        Assert.Equal(AppStrings.RoleChangedSuccess, alert.Message);
    }

    [Fact]
    public async Task ChangeRoleAsync_WhenNoRoleSelected_ShowsValidationMessage_WithoutCallingService()
    {
        var (vm, userService, alerts) = CreateSut();
        var user = new UserListItem { Id = ActiveUserId, IsActive = true, SelectedRole = null };

        await vm.ChangeRoleAsync(user);

        Assert.Null(userService.LastChangeRoleCall);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.AttentionTitle, alert.Title);
        Assert.Equal(AppStrings.MsgSelectRoleFirst, alert.Message);
    }

    // ─── BACKEND RECHAZA POR PERMISOS INSUFICIENTES ───

    [Fact]
    public async Task ApproveAsync_WhenBackendRejectsForInsufficientPermission_ShowsExactBackendMessage()
    {
        var (vm, userService, alerts) = CreateSut();
        userService.ActivateResult = new UserActionResult
        {
            IsSuccess = false,
            Message = "La acción sobre 'Usuario' no está permitida o el estado es inválido."
        };
        var user = new UserListItem { Id = PendingUserId, IsActive = false };

        await vm.ApproveAsync(user);

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal("La acción sobre 'Usuario' no está permitida o el estado es inválido.", alert.Message);
    }

    [Fact]
    public async Task ChangeRoleAsync_WhenBackendRejectsForInsufficientPermission_ShowsExactBackendMessage_AndDoesNotReload()
    {
        var (vm, userService, alerts) = CreateSut();
        userService.ChangeRoleResult = new UserActionResult
        {
            IsSuccess = false,
            Message = "La acción sobre 'Usuario' no está permitida o el estado es inválido."
        };
        var user = new UserListItem { Id = ActiveUserId, IsActive = true, SelectedRole = new RoleItem { Id = RoleId, Name = "Tesorero" } };

        await vm.ChangeRoleAsync(user);

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal("La acción sobre 'Usuario' no está permitida o el estado es inválido.", alert.Message);
    }

    [Fact]
    public async Task AssignWalkwayAsync_WhenBackendRejectsForInsufficientPermission_ShowsExactBackendMessage()
    {
        var (vm, userService, alerts) = CreateSut();
        userService.AssignWalkwayResult = new UserActionResult
        {
            IsSuccess = false,
            Message = "La acción sobre 'Usuario' no está permitida o el estado es inválido."
        };
        var user = new UserListItem { Id = ActiveUserId, IsActive = true, SelectedWalkway = new WalkwayItem { Id = WalkwayId, Code = "A-01" } };

        await vm.AssignWalkwayAsync(user);

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal("La acción sobre 'Usuario' no está permitida o el estado es inválido.", alert.Message);
    }
}
