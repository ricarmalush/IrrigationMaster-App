using IrrigationMaster.Mobile.Application.Constants;
using IrrigationMaster.Mobile.Application.Features.Models.Auth;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level1_Core.Login;
using IrrigationMaster.UI.Maui.Tests.TestDoubles;

namespace IrrigationMaster.UI.Maui.Tests;

public class LoginViewModelTests
{
    private static (LoginViewModel ViewModel, FakeAuthService Auth, FakeCurrentSession Session, RecordingAlertService Alerts, RecordingNavigationService Navigation) CreateSut()
    {
        var auth = new FakeAuthService();
        var session = new FakeCurrentSession();
        var alerts = new RecordingAlertService();
        var navigation = new RecordingNavigationService();

        var viewModel = new LoginViewModel(auth, session, alerts, navigation);

        return (viewModel, auth, session, alerts, navigation);
    }

    [Fact]
    public async Task ExecuteLoginAsync_OnSuccess_EstablishesSessionAndNavigatesToAdminMenu()
    {
        var (vm, auth, session, alerts, navigation) = CreateSut();
        auth.ResponseToReturn = new LoginResponse { IsSuccess = true, Message = "OK", Data = "un-jwt-cualquiera" };
        vm.Email = "admin@elsaso.test";
        vm.Password = "clave-valida";

        await vm.ExecuteLoginAsync();

        Assert.False(vm.IsLoading);
        Assert.Equal("un-jwt-cualquiera", session.EstablishedWithToken);
        Assert.Equal(["//AdminMenuPage"], navigation.Routes);
        Assert.Empty(alerts.Calls);
    }

    [Fact]
    public async Task ExecuteLoginAsync_WithInvalidCredentials_ShowsBackendMessageAndDoesNotNavigate()
    {
        var (vm, auth, session, alerts, navigation) = CreateSut();
        auth.ResponseToReturn = new LoginResponse { IsSuccess = false, Message = "Credenciales inválidas" };
        vm.Email = "admin@elsaso.test";
        vm.Password = "clave-incorrecta";

        await vm.ExecuteLoginAsync();

        Assert.False(vm.IsLoading);
        Assert.Null(session.EstablishedWithToken);
        Assert.Empty(navigation.Routes);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal("Credenciales inválidas", alert.Message);
    }

    [Fact]
    public async Task ExecuteLoginAsync_OnNetworkFailure_ShowsNetworkErrorMessage()
    {
        // IAuthService/ApiService nunca lanza: un fallo de red vuelve como IsSuccess=false
        // con ServiceMessages.NetworkConnectionError (ver ApiServiceTests). Se reproduce igual aquí.
        var (vm, auth, session, alerts, navigation) = CreateSut();
        auth.ResponseToReturn = new LoginResponse { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        vm.Email = "admin@elsaso.test";
        vm.Password = "clave-valida";

        await vm.ExecuteLoginAsync();

        Assert.False(vm.IsLoading);
        Assert.Null(session.EstablishedWithToken);
        Assert.Empty(navigation.Routes);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal(ServiceMessages.NetworkConnectionError, alert.Message);
    }

    [Fact]
    public async Task ExecuteLoginAsync_WithNullResponse_ShowsNetworkFallbackMessage()
    {
        var (vm, auth, session, alerts, navigation) = CreateSut();
        auth.ResponseToReturn = null;
        vm.Email = "admin@elsaso.test";
        vm.Password = "clave-valida";

        await vm.ExecuteLoginAsync();

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal(AppStrings.NetworkFallbackError, alert.Message);
    }

    [Theory]
    [InlineData("", "clave")]
    [InlineData("correo@test.com", "")]
    [InlineData("   ", "   ")]
    public async Task ExecuteLoginAsync_WithMissingFields_DoesNotCallAuthService(string email, string password)
    {
        var (vm, auth, session, alerts, navigation) = CreateSut();
        vm.Email = email;
        vm.Password = password;

        await vm.ExecuteLoginAsync();

        Assert.Null(auth.LastCall);
        Assert.Empty(navigation.Routes);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.AttentionTitle, alert.Title);
    }

    [Fact]
    public async Task ExecuteNavigateToRegisterAsync_NavigatesToRegisterRoute()
    {
        var (vm, _, _, _, navigation) = CreateSut();

        await vm.ExecuteNavigateToRegisterAsync();

        Assert.Equal(["RegisterPage"], navigation.Routes);
    }
}
