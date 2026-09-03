using IrrigationMaster.UI.Maui.Features.Level1_Core.Welcome;
using IrrigationMaster.UI.Maui.Tests.TestDoubles;

namespace IrrigationMaster.UI.Maui.Tests;

public class WelcomeViewModelTests
{
    private static (WelcomeViewModel ViewModel, FakeTokenStorage TokenStorage, RecordingNavigationService NavigationService) CreateSut()
    {
        var tokenStorage = new FakeTokenStorage();
        var navigationService = new RecordingNavigationService();
        var viewModel = new WelcomeViewModel(tokenStorage, navigationService);
        return (viewModel, tokenStorage, navigationService);
    }

    [Fact]
    public async Task DetermineDestinationRouteAsync_WhenNoTokenStored_ReturnsLoginRoute()
    {
        var (vm, _, _) = CreateSut();

        var route = await vm.DetermineDestinationRouteAsync();

        Assert.Equal(WelcomeViewModel.LoginRoute, route);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task DetermineDestinationRouteAsync_WhenTokenIsMissingOrBlank_ReturnsLoginRoute(string? blankToken)
    {
        var (vm, tokenStorage, _) = CreateSut();
        tokenStorage.StoredToken = blankToken;

        var route = await vm.DetermineDestinationRouteAsync();

        Assert.Equal(WelcomeViewModel.LoginRoute, route);
    }

    [Fact]
    public async Task DetermineDestinationRouteAsync_WhenTokenStored_ReturnsHomeRoute()
    {
        var (vm, tokenStorage, _) = CreateSut();
        tokenStorage.StoredToken = "un-jwt-cualquiera";

        var route = await vm.DetermineDestinationRouteAsync();

        Assert.Equal(WelcomeViewModel.HomeRoute, route);
    }

    [Fact]
    public async Task NavigateToDestinationAsync_WhenNoSession_NavigatesToLoginRoute()
    {
        var (vm, _, navigationService) = CreateSut();

        await vm.NavigateToDestinationCommand.ExecuteAsync(null);

        Assert.Equal([WelcomeViewModel.LoginRoute], navigationService.Routes);
    }

    [Fact]
    public async Task NavigateToDestinationAsync_WhenSessionActive_NavigatesToHomeRoute()
    {
        var (vm, tokenStorage, navigationService) = CreateSut();
        tokenStorage.StoredToken = "un-jwt-cualquiera";

        await vm.NavigateToDestinationCommand.ExecuteAsync(null);

        Assert.Equal([WelcomeViewModel.HomeRoute], navigationService.Routes);
    }
}
