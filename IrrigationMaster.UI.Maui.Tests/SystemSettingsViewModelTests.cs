using IrrigationMaster.Mobile.Application.Constants;
using IrrigationMaster.Mobile.Infrastructure;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.AdminConsole;
using IrrigationMaster.UI.Maui.Tests.TestDoubles;
using System.Net;
using static IrrigationMaster.UI.Maui.Tests.TestDoubles.RoutingFakeHttpMessageHandler;

namespace IrrigationMaster.UI.Maui.Tests;

public class SystemSettingsViewModelTests
{
    private const string FakeBaseUrl = "https://fake-backend.test/api/v1/";
    private const string CreatedResponseJson = """{ "isSuccess": true, "message": "Operación completada exitosamente.", "data": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }""";

    private static (SystemSettingsViewModel ViewModel, RoutingFakeHttpMessageHandler Handler, RecordingAlertService Alerts, FakeCurrentSession Session) CreateSut(string? organizationId = null)
    {
        var handler = new RoutingFakeHttpMessageHandler();
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(FakeBaseUrl) };
        var apiService = new ApiService(httpClient, new FakeTokenStorage { StoredToken = "token-123" });
        var alerts = new RecordingAlertService();
        var session = new FakeCurrentSession { OrganizationIdToReturn = organizationId };

        var viewModel = new SystemSettingsViewModel(apiService, alerts, session);

        return (viewModel, handler, alerts, session);
    }

    private static void SetValidOrganizationFields(SystemSettingsViewModel vm)
    {
        vm.OrgName = "Regantes El Saso";
        vm.OrgTaxId = "G50123456";
        vm.OrgStreet = "Camino Real s/n";
        vm.OrgCity = "El Saso";
        vm.OrgStateOrProvince = "Huesca";
        vm.OrgPostalCode = "22300";
        vm.SelectedCountry = new CountryItem { Id = Guid.NewGuid(), Name = "España" };
    }

    // ─── MI ORGANIZACIÓN: nombre + InvitationCode de la organización del usuario logueado ───

    [Fact]
    public async Task LoadMyOrganizationAsync_OnSuccess_PopulatesNameAndInvitationCode()
    {
        var organizationId = Guid.NewGuid();
        var (vm, handler, _, _) = CreateSut(organizationId.ToString());
        const string responseJson = """
        {
            "isSuccess": true,
            "message": "OK",
            "data": { "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6", "name": "Regantes El Saso", "invitationCode": "H4RN8WEQ" }
        }
        """;
        handler.AddRoute(IsGetTo($"organizations/Get/{organizationId}"), HttpStatusCode.OK, responseJson);

        await vm.LoadMyOrganizationAsync();

        Assert.Equal("Regantes El Saso", vm.MyOrganizationName);
        Assert.Equal("H4RN8WEQ", vm.MyOrganizationInvitationCode);
    }

    [Fact]
    public async Task LoadMyOrganizationAsync_WhenSessionHasNoOrganizationId_DoesNotCallApi_AndLeavesFieldsEmpty()
    {
        var (vm, handler, _, _) = CreateSut(organizationId: null);

        await vm.LoadMyOrganizationAsync();

        Assert.DoesNotContain(handler.Requests, r => r.RequestUri!.AbsolutePath.Contains("organizations/Get", StringComparison.OrdinalIgnoreCase));
        Assert.Equal(string.Empty, vm.MyOrganizationName);
        Assert.Equal(string.Empty, vm.MyOrganizationInvitationCode);
    }

    // ─── IsSuperAdmin: decide si SystemSettingsPage oculta la pestaña "Entidad" ───

    [Fact]
    public async Task LoadCurrentUserRoleAsync_WhenSessionRoleIsSuperAdmin_SetsIsSuperAdminTrue()
    {
        var (vm, _, _, session) = CreateSut();
        session.RoleToReturn = "SUPERADMIN";

        await vm.LoadCurrentUserRoleAsync();

        Assert.True(vm.IsSuperAdmin);
    }

    [Theory]
    [InlineData("PRESIDENTE")]
    [InlineData("VECINO")]
    [InlineData(null)]
    public async Task LoadCurrentUserRoleAsync_WhenSessionRoleIsNotSuperAdmin_SetsIsSuperAdminFalse(string? role)
    {
        var (vm, _, _, session) = CreateSut();
        session.RoleToReturn = role;

        await vm.LoadCurrentUserRoleAsync();

        Assert.False(vm.IsSuperAdmin);
    }

    // ─── ORGANIZACIÓN: éxito, fallo de red, fallo de validación del backend ───

    [Fact]
    public async Task ExecuteSaveOrganizationAsync_OnSuccess_ShowsSuccessAlert()
    {
        var (vm, handler, alerts, _) = CreateSut();
        handler.AddRoute(IsPostTo("organizations/Create"), HttpStatusCode.Created, CreatedResponseJson);
        SetValidOrganizationFields(vm);

        await vm.ExecuteSaveOrganizationAsync();

        Assert.False(vm.IsLoading);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.SuccessTitle, alert.Title);
    }

    [Fact]
    public async Task ExecuteSaveOrganizationAsync_OnNetworkFailure_ShowsNetworkErrorAlert()
    {
        var (vm, handler, alerts, _) = CreateSut();
        handler.AddThrowingRoute(IsPostTo("organizations/Create"));
        SetValidOrganizationFields(vm);

        await vm.ExecuteSaveOrganizationAsync();

        Assert.False(vm.IsLoading);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal(ServiceMessages.NetworkConnectionError, alert.Message);
    }

    [Fact]
    public async Task ExecuteSaveOrganizationAsync_OnBackendValidationFailure_ShowsBackendErrorsInAlert()
    {
        var (vm, handler, alerts, _) = CreateSut();
        const string errorsJson = """
        {
            "isSuccess": false,
            "message": "Datos inválidos",
            "errors": [
                { "propertyMessage": "TaxId", "errorMessage": "El NIF ya está registrado" }
            ]
        }
        """;
        handler.AddRoute(IsPostTo("organizations/Create"), HttpStatusCode.BadRequest, errorsJson);
        SetValidOrganizationFields(vm);

        await vm.ExecuteSaveOrganizationAsync();

        Assert.False(vm.IsLoading);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Contains("TaxId", alert.Message);
        Assert.Contains("El NIF ya está registrado", alert.Message);
    }

    [Fact]
    public async Task ExecuteSaveOrganizationAsync_WithMissingCountry_DoesNotCallApi()
    {
        var (vm, handler, alerts, _) = CreateSut();
        SetValidOrganizationFields(vm);
        vm.SelectedCountry = null;

        await vm.ExecuteSaveOrganizationAsync();

        Assert.DoesNotContain(handler.Requests, r => r.RequestUri!.AbsolutePath.EndsWith("organizations/Create"));
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.AttentionTitle, alert.Title);
    }

    // ─── SECTOR HIDRÁULICO: éxito (ya no manda OrganizationId) y fallo de validación ───

    [Fact]
    public async Task ExecuteSaveHydraulicSectorAsync_OnSuccess_ShowsSuccessAlertAndClearsFields()
    {
        var (vm, handler, alerts, _) = CreateSut();
        handler.AddRoute(IsPostTo("hydraulicsectors/Create"), HttpStatusCode.Created, CreatedResponseJson);
        vm.SectorName = "Sector Norte";
        vm.SectorAreaSize = "150.5";

        await vm.ExecuteSaveHydraulicSectorAsync();

        Assert.False(vm.IsLoading);
        Assert.Equal(string.Empty, vm.SectorName);
        Assert.Equal(string.Empty, vm.SectorAreaSize);
        Assert.Contains(alerts.Calls, a => a.Title == AppStrings.SuccessTitle);
    }

    [Fact]
    public async Task ExecuteSaveHydraulicSectorAsync_OnBackendValidationFailure_ShowsBackendErrorsInAlert()
    {
        var (vm, handler, alerts, _) = CreateSut();
        const string errorsJson = """
        { "isSuccess": false, "message": "No se pudo crear el sector", "errors": [ { "propertyMessage": "Name", "errorMessage": "Ya existe un sector con ese nombre" } ] }
        """;
        handler.AddRoute(IsPostTo("hydraulicsectors/Create"), HttpStatusCode.BadRequest, errorsJson);
        vm.SectorName = "Sector Norte";
        vm.SectorAreaSize = "150.5";

        await vm.ExecuteSaveHydraulicSectorAsync();

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Contains("Ya existe un sector con ese nombre", alert.Message);
    }

    // ─── ANDADOR: éxito con sector seleccionado, y validación sin sector ───

    [Fact]
    public async Task ExecuteSaveWalkwayAsync_OnSuccess_ShowsSuccessAlert()
    {
        var (vm, handler, alerts, _) = CreateSut();
        handler.AddRoute(IsPostTo("walkways/Create"), HttpStatusCode.Created, CreatedResponseJson);
        vm.WalkwayCode = "A-01";
        vm.WalkwayLength = "400";
        vm.SelectedHydraulicSector = new HydraulicSectorItem { Id = Guid.NewGuid(), Name = "Sector Norte" };

        await vm.ExecuteSaveWalkwayAsync();

        Assert.False(vm.IsLoading);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.SuccessTitle, alert.Title);
    }

    [Fact]
    public async Task ExecuteSaveWalkwayAsync_WithoutSelectedSector_DoesNotCallApi()
    {
        var (vm, handler, alerts, _) = CreateSut();
        vm.WalkwayCode = "A-01";
        vm.WalkwayLength = "400";
        vm.SelectedHydraulicSector = null;

        await vm.ExecuteSaveWalkwayAsync();

        Assert.DoesNotContain(handler.Requests, r => r.RequestUri!.AbsolutePath.EndsWith("walkways/Create"));
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.AttentionTitle, alert.Title);
    }

    [Fact]
    public async Task ExecuteSaveWalkwayAsync_OnNetworkFailure_ShowsNetworkErrorAlert()
    {
        var (vm, handler, alerts, _) = CreateSut();
        handler.AddThrowingRoute(IsPostTo("walkways/Create"));
        vm.WalkwayCode = "A-01";
        vm.WalkwayLength = "400";
        vm.SelectedHydraulicSector = new HydraulicSectorItem { Id = Guid.NewGuid(), Name = "Sector Norte" };

        await vm.ExecuteSaveWalkwayAsync();

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal(ServiceMessages.NetworkConnectionError, alert.Message);
    }
}
