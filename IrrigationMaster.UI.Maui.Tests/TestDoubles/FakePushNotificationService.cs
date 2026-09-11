using IrrigationMaster.Mobile.Application.Features.Models.Devices;
using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class FakePushNotificationService : IPushNotificationService
{
    public string? TokenToReturn { get; set; }

    public event EventHandler<string>? TokenChanged;
    public event EventHandler<PushNotificationInfo>? NotificationReceived;
    public event EventHandler<PushNotificationInfo>? NotificationTapped;

    public Task<string?> GetTokenAsync() => Task.FromResult(TokenToReturn);

    public void RaiseTokenChanged(string token) => TokenChanged?.Invoke(this, token);
    public void RaiseNotificationReceived(PushNotificationInfo notification) => NotificationReceived?.Invoke(this, notification);
    public void RaiseNotificationTapped(PushNotificationInfo notification) => NotificationTapped?.Invoke(this, notification);
}
