using IrrigationMaster.Mobile.Application.Features.Models.Devices;
using IrrigationMaster.UI.Maui.Services;
using IrrigationMaster.UI.Maui.Tests.TestDoubles;

namespace IrrigationMaster.UI.Maui.Tests;

public class PushNotificationCoordinatorTests
{
    private static (PushNotificationCoordinator Coordinator, FakePushNotificationService Push, FakeUserDeviceService Devices, FakeTokenStorage TokenStorage, RecordingNavigationService Navigation) CreateSut()
    {
        var push = new FakePushNotificationService();
        var devices = new FakeUserDeviceService();
        var tokenStorage = new FakeTokenStorage();
        var navigation = new RecordingNavigationService();

        var coordinator = new PushNotificationCoordinator(push, devices, tokenStorage, navigation);

        return (coordinator, push, devices, tokenStorage, navigation);
    }

    [Fact]
    public async Task TokenChanged_WithActiveSession_RegistersRefreshedToken()
    {
        var (_, push, devices, tokenStorage, _) = CreateSut();
        tokenStorage.StoredToken = "sesion-activa";

        push.RaiseTokenChanged("token-refrescado");
        await Task.Delay(50); // el handler es async void: deja que la continuación corra

        Assert.NotNull(devices.LastCall);
        Assert.Equal("token-refrescado", devices.LastCall!.Value.DeviceToken);
    }

    [Fact]
    public async Task TokenChanged_WithoutActiveSession_DoesNotRegister()
    {
        // El backend resuelve el dueño del dispositivo desde el JWT -- sin sesión no hay a quién
        // asociarlo, se reenviará solo en el próximo login.
        var (_, push, devices, tokenStorage, _) = CreateSut();
        tokenStorage.StoredToken = null;

        push.RaiseTokenChanged("token-refrescado");
        await Task.Delay(50);

        Assert.Null(devices.LastCall);
    }

    [Fact]
    public async Task NotificationTapped_WithRoute_NavigatesToIt()
    {
        var (_, push, _, _, navigation) = CreateSut();
        var notification = new PushNotificationInfo
        {
            Data = new Dictionary<string, string> { ["route"] = "//AdminMenuPage" }
        };

        push.RaiseNotificationTapped(notification);
        await Task.Delay(50);

        Assert.Equal(["//AdminMenuPage"], navigation.Routes);
    }

    [Fact]
    public async Task NotificationTapped_WithoutRoute_DoesNotNavigate()
    {
        var (_, push, _, _, navigation) = CreateSut();
        var notification = new PushNotificationInfo { Title = "Aviso general", Body = "Sin deep-link" };

        push.RaiseNotificationTapped(notification);
        await Task.Delay(50);

        Assert.Empty(navigation.Routes);
    }
}
