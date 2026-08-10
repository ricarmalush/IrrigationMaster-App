using IrrigationMaster.Mobile.Application.Features.Models.Structure;
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

    private static (UserManagementViewModel ViewModel, FakeUserManagementService UserService, RecordingAlertService Alerts) CreateSut(string role = "PRESIDENTE")
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
                    OrganizationId = OrganizationId, Role = "Vecino", IsActive = false,
                    OrganizationName = "Regantes El Saso"
                }
            ]
        };
        var alerts = new RecordingAlertService();
        var structureService = new FakeStructureService();
        var session = new FakeCurrentSession { RoleToReturn = role };

        var viewModel = new UserManagementViewModel(userService, structureService, alerts, session);

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
        Assert.Equal("Regantes El Saso", user.OrganizationName);
        Assert.True(user.ShowApprove); // pendiente -> debe ofrecer "Aprobar"
        Assert.Single(vm.Roles);
        Assert.Single(vm.Walkways);
    }

    [Fact]
    public async Task LoadAsync_WithUsersFromDifferentOrganizations_PreservesEachUsersOwnOrganizationName()
    {
        // Caso SUPERADMIN: la lista puede mezclar usuarios de organizaciones distintas -- cada
        // tarjeta debe mostrar la organización que le corresponde a ESE usuario, no una fija.
        var userService = new FakeUserManagementService
        {
            RolesToReturn = [],
            WalkwaysToReturn = [],
            UsersToReturn =
            [
                new AppUserDto { Id = Guid.NewGuid(), FirstName = "Ana", LastName = "García", Email = "ana@test.com", OrganizationName = "Regantes El Saso", IsActive = true },
                new AppUserDto { Id = Guid.NewGuid(), FirstName = "Luis", LastName = "Pérez", Email = "luis@test.com", OrganizationName = "Regantes Ajena", IsActive = true }
            ]
        };
        var vm = new UserManagementViewModel(userService, new FakeStructureService(), new RecordingAlertService(), new FakeCurrentSession { RoleToReturn = "SUPERADMIN" });

        await vm.LoadAsync();

        Assert.Equal(2, vm.Users.Count);
        Assert.Contains(vm.Users, u => u.FullName == "Ana García" && u.OrganizationName == "Regantes El Saso");
        Assert.Contains(vm.Users, u => u.FullName == "Luis Pérez" && u.OrganizationName == "Regantes Ajena");
    }

    [Fact]
    public async Task LoadAsync_ExcludesOnlySuperAdminRoleByCode_KeepsOtherGlobalTemplateRoles()
    {
        // Caso que motivó el arreglo: VECINO y PRESIDENTE también tienen OrganizationId ==
        // Guid.Empty (roles de plantilla globales por diseño), igual que SUPERADMIN -- el filtro
        // debe descartar solo SUPERADMIN por Code, no "cualquier rol con OrganizationId vacío".
        var vecinoRoleId = Guid.NewGuid();
        var presidenteRoleId = Guid.NewGuid();
        var superAdminRoleId = Guid.NewGuid();

        var userService = new FakeUserManagementService
        {
            RolesToReturn =
            [
                new RoleDto { Id = vecinoRoleId, Name = "Vecino", Code = "VECINO", OrganizationId = Guid.Empty },
                new RoleDto { Id = presidenteRoleId, Name = "Presidente", Code = "PRESIDENTE", OrganizationId = Guid.Empty },
                new RoleDto { Id = superAdminRoleId, Name = "Super Administrador", Code = "SUPERADMIN", OrganizationId = Guid.Empty }
            ],
            WalkwaysToReturn = [],
            UsersToReturn = []
        };
        var vm = new UserManagementViewModel(userService, new FakeStructureService(), new RecordingAlertService(), new FakeCurrentSession { RoleToReturn = "PRESIDENTE" });

        await vm.LoadAsync();

        Assert.Equal(2, vm.Roles.Count);
        Assert.Contains(vm.Roles, r => r.Id == vecinoRoleId);
        Assert.Contains(vm.Roles, r => r.Id == presidenteRoleId);
        Assert.DoesNotContain(vm.Roles, r => r.Id == superAdminRoleId);
    }

    // ─── FILTRO: Organización (solo SUPERADMIN) ───

    [Fact]
    public void ShowOrganizationFilter_IsTrue_ForSuperAdmin()
    {
        var (vm, _, _) = CreateSut(role: "SUPERADMIN");

        Assert.True(vm.ShowOrganizationFilter);
    }

    [Theory]
    [InlineData("PRESIDENTE")]
    [InlineData("VECINO")]
    public void ShowOrganizationFilter_IsFalse_ForNonSuperAdminRoles(string role)
    {
        var (vm, _, _) = CreateSut(role);

        Assert.False(vm.ShowOrganizationFilter);
    }

    [Fact]
    public async Task LoadAsync_ForSuperAdmin_PopulatesOrganizationFilters_WithAllOptionFirst()
    {
        var elSaso = new OrganizationDto { Id = Guid.NewGuid(), Name = "Asociación de Vecinos El Saso (AVES)" };
        var horizonteVerde = new OrganizationDto { Id = Guid.NewGuid(), Name = "Cooperativa Horizonte Verde" };
        var userService = new FakeUserManagementService();
        var structureService = new FakeStructureService { OrganizationsToReturn = [elSaso, horizonteVerde] };
        var vm = new UserManagementViewModel(userService, structureService, new RecordingAlertService(), new FakeCurrentSession { RoleToReturn = "SUPERADMIN" });

        await vm.LoadAsync();

        Assert.Equal(3, vm.OrganizationFilters.Count);
        Assert.Null(vm.OrganizationFilters[0].Id);
        Assert.Equal("Todas las organizaciones", vm.OrganizationFilters[0].Name);
        Assert.Contains(vm.OrganizationFilters, f => f.Id == elSaso.Id && f.Name == elSaso.Name);
        Assert.Contains(vm.OrganizationFilters, f => f.Id == horizonteVerde.Id && f.Name == horizonteVerde.Name);
        Assert.Same(vm.OrganizationFilters[0], vm.SelectedOrganizationFilter);
    }

    [Fact]
    public async Task LoadAsync_ForNonSuperAdmin_DoesNotPopulateOrganizationFilters()
    {
        var userService = new FakeUserManagementService();
        var structureService = new FakeStructureService { OrganizationsToReturn = [new OrganizationDto { Id = Guid.NewGuid(), Name = "Cooperativa Horizonte Verde" }] };
        var vm = new UserManagementViewModel(userService, structureService, new RecordingAlertService(), new FakeCurrentSession { RoleToReturn = "PRESIDENTE" });

        await vm.LoadAsync();

        Assert.Empty(vm.OrganizationFilters);
    }

    [Fact]
    public async Task LoadAsync_WithAllOrganizationsSelected_RequestsUsers_WithoutOrganizationFilter()
    {
        var userService = new FakeUserManagementService();
        var structureService = new FakeStructureService { OrganizationsToReturn = [new OrganizationDto { Id = Guid.NewGuid(), Name = "Cooperativa Horizonte Verde" }] };
        var vm = new UserManagementViewModel(userService, structureService, new RecordingAlertService(), new FakeCurrentSession { RoleToReturn = "SUPERADMIN" });

        await vm.LoadAsync();

        Assert.Null(userService.LastOrganizationIdFilter);
    }

    [Fact]
    public async Task SelectingAnOrganization_ReloadsUsers_WithThatOrganizationId()
    {
        var horizonteVerde = new OrganizationDto { Id = Guid.NewGuid(), Name = "Cooperativa Horizonte Verde" };
        var userService = new FakeUserManagementService();
        var structureService = new FakeStructureService { OrganizationsToReturn = [horizonteVerde] };
        var vm = new UserManagementViewModel(userService, structureService, new RecordingAlertService(), new FakeCurrentSession { RoleToReturn = "SUPERADMIN" });
        await vm.LoadAsync();

        vm.SelectedOrganizationFilter = vm.OrganizationFilters.Single(f => f.Id == horizonteVerde.Id);
        await Task.Yield(); // OnSelectedOrganizationFilterChanged dispara LoadAsync sin esperarlo (_ = LoadAsync())

        Assert.Equal(horizonteVerde.Id, userService.LastOrganizationIdFilter);
        Assert.Equal(horizonteVerde.Name, vm.OrganizationFilterSelectionDisplay);
    }

    [Fact]
    public async Task SelectingTodasAfterAnOrganization_ReloadsUsers_WithoutOrganizationFilter()
    {
        var horizonteVerde = new OrganizationDto { Id = Guid.NewGuid(), Name = "Cooperativa Horizonte Verde" };
        var userService = new FakeUserManagementService();
        var structureService = new FakeStructureService { OrganizationsToReturn = [horizonteVerde] };
        var vm = new UserManagementViewModel(userService, structureService, new RecordingAlertService(), new FakeCurrentSession { RoleToReturn = "SUPERADMIN" });
        await vm.LoadAsync();
        vm.SelectedOrganizationFilter = vm.OrganizationFilters.Single(f => f.Id == horizonteVerde.Id);
        await Task.Yield();

        vm.SelectedOrganizationFilter = vm.OrganizationFilters.Single(f => f.Id == null);
        await Task.Yield();

        Assert.Null(userService.LastOrganizationIdFilter);
        Assert.Equal("Todas las organizaciones", vm.OrganizationFilterSelectionDisplay);
    }

    [Fact]
    public void OrganizationFilterSelectionDisplay_WithNoSelection_ShowsPlaceholder()
    {
        var (vm, _, _) = CreateSut(role: "SUPERADMIN");

        Assert.Equal("Todas las organizaciones", vm.OrganizationFilterSelectionDisplay);
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

    // ─── SELECCIÓN VISIBLE (Picker en Windows no refresca su propio texto: dotnet/maui #5038,
    // #24369 -- RoleSelectionDisplay/WalkwaySelectionDisplay son el reemplazo fiable que se
    // bindea en un Label aparte) ───

    [Fact]
    public void RoleSelectionDisplay_WithNoSelection_ShowsPlaceholder()
    {
        var user = new UserListItem();

        Assert.Equal("Selecciona un rol", user.RoleSelectionDisplay);
    }

    [Fact]
    public void WalkwaySelectionDisplay_WithNoSelection_ShowsPlaceholder()
    {
        var user = new UserListItem();

        Assert.Equal("Sin asignar", user.WalkwaySelectionDisplay);
    }

    [Fact]
    public void RoleSelectionDisplay_ReflectsSelectedRole_AndRaisesPropertyChanged()
    {
        var user = new UserListItem();
        var raisedFor = new List<string>();
        user.PropertyChanged += (_, e) => raisedFor.Add(e.PropertyName!);

        user.SelectedRole = new RoleItem { Id = RoleId, Name = "Tesorero" };

        Assert.Equal("Tesorero", user.RoleSelectionDisplay);
        Assert.Contains(nameof(UserListItem.RoleSelectionDisplay), raisedFor);
    }

    [Fact]
    public void WalkwaySelectionDisplay_ReflectsSelectedWalkway_AndRaisesPropertyChanged()
    {
        var user = new UserListItem();
        var raisedFor = new List<string>();
        user.PropertyChanged += (_, e) => raisedFor.Add(e.PropertyName!);

        user.SelectedWalkway = new WalkwayItem { Id = WalkwayId, Code = "A-01" };

        Assert.Equal("A-01", user.WalkwaySelectionDisplay);
        Assert.Contains(nameof(UserListItem.WalkwaySelectionDisplay), raisedFor);
    }
}
