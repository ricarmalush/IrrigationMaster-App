using IrrigationMaster.Mobile.Application.Common.Dtos;
using IrrigationMaster.Mobile.Application.Constants;
using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.Infrastructure;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level1_Core.Register;
using IrrigationMaster.UI.Maui.Tests.TestDoubles;

namespace IrrigationMaster.UI.Maui.Tests;

public class RegisterViewModelTests
{
    private static (RegisterViewModel ViewModel, FakeRegistrationService Registration, RecordingAlertService Alerts, RecordingNavigationService Navigation) CreateSut()
    {
        var registration = new FakeRegistrationService();
        var alerts = new RecordingAlertService();
        var navigation = new RecordingNavigationService();

        var viewModel = new RegisterViewModel(registration, alerts, navigation);

        return (viewModel, registration, alerts, navigation);
    }

    private static void SetValidFields(RegisterViewModel vm)
    {
        vm.FirstName = "Ana";
        vm.LastName = "García";
        vm.Email = "ana@correo.test";
        vm.Password = "clave12345";
    }

    [Fact]
    public async Task RegisterAsync_OnSuccess_UsesConfiguredTenantValues_AndNavigatesBack()
    {
        var (vm, registration, alerts, navigation) = CreateSut();
        registration.ResponseToReturn = new StructureOperationResult { IsSuccess = true };
        SetValidFields(vm);

        await vm.RegisterAsync();

        Assert.False(vm.IsBusy);
        Assert.NotNull(registration.LastRequest);
        Assert.Equal(TenantConfig.DefaultOrganizationId, registration.LastRequest!.OrganizationId);
        Assert.Equal(TenantConfig.DefaultVecinoRoleId, registration.LastRequest.RoleId);
        Assert.Equal("Ana", registration.LastRequest.FirstName);
        Assert.Equal("García", registration.LastRequest.LastName);
        Assert.Equal("ana@correo.test", registration.LastRequest.Email);
        Assert.Equal([".."], navigation.Routes);
        Assert.Contains(alerts.Calls, a => a.Title == AppStrings.SuccessTitle);
    }

    [Fact]
    public async Task RegisterAsync_WithMissingFields_DoesNotCallApi()
    {
        var (vm, registration, alerts, navigation) = CreateSut();
        vm.FirstName = "Ana";
        // LastName/Email/Password vacíos

        await vm.RegisterAsync();

        Assert.Null(registration.LastRequest);
        Assert.Empty(navigation.Routes);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.AttentionTitle, alert.Title);
    }

    [Fact]
    public async Task RegisterAsync_OnBackendValidationFailure_ShowsBackendErrorsInAlert()
    {
        var (vm, registration, alerts, navigation) = CreateSut();
        registration.ResponseToReturn = new StructureOperationResult
        {
            IsSuccess = false,
            Message = "Datos inválidos",
            Errors = [new ApiError { PropertyMessage = "Email", ErrorMessage = "Ya existe una cuenta con ese correo" }]
        };
        SetValidFields(vm);

        await vm.RegisterAsync();

        Assert.False(vm.IsBusy);
        Assert.Empty(navigation.Routes);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Contains("Email", alert.Message);
        Assert.Contains("Ya existe una cuenta con ese correo", alert.Message);
    }

    [Fact]
    public async Task RegisterAsync_OnNetworkFailure_ShowsNetworkErrorMessage()
    {
        // ApiService.RegisterAsync nunca lanza: un fallo de red vuelve como IsSuccess=false
        // con ServiceMessages.NetworkConnectionError (ver ApiServiceTests). Se reproduce igual aquí.
        var (vm, registration, alerts, navigation) = CreateSut();
        registration.ResponseToReturn = new StructureOperationResult { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        SetValidFields(vm);

        await vm.RegisterAsync();

        Assert.Empty(navigation.Routes);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal(ServiceMessages.NetworkConnectionError, alert.Message);
    }

    // ─── SELECTOR DE ANDADOR (WALKWAY) ───

    [Fact]
    public async Task LoadWalkwaysAsync_PopulatesWalkwaysFromApi()
    {
        var (vm, registration, _, _) = CreateSut();
        var walkwayId = Guid.NewGuid();
        registration.WalkwaysToReturn = [new PublicWalkwayDto { Id = walkwayId, Code = "A-01" }];

        await vm.LoadWalkwaysAsync();

        var walkway = Assert.Single(vm.Walkways);
        Assert.Equal(walkwayId, walkway.Id);
        Assert.Equal("A-01", walkway.Code);
    }

    [Fact]
    public async Task RegisterAsync_WithSelectedWalkway_SendsWalkwayId()
    {
        var (vm, registration, _, _) = CreateSut();
        SetValidFields(vm);
        var walkwayId = Guid.NewGuid();
        vm.SelectedWalkway = new WalkwayItem { Id = walkwayId, Code = "A-01" };

        await vm.RegisterAsync();

        Assert.Equal(walkwayId, registration.LastRequest!.WalkwayId);
    }

    [Fact]
    public async Task RegisterAsync_WithoutSelectedWalkway_SendsNullWalkwayId()
    {
        var (vm, registration, _, _) = CreateSut();
        SetValidFields(vm);

        await vm.RegisterAsync();

        Assert.Null(registration.LastRequest!.WalkwayId);
    }
}
