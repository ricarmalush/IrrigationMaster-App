using IrrigationMaster.Mobile.Application.Features.Models.Devices;
using IrrigationMaster.Mobile.Application.Interfaces;
using Plugin.Firebase.CloudMessaging;

namespace IrrigationMaster.UI.Maui.Services;

// Implementación real sobre Plugin.Firebase.CloudMessaging (Android/iOS). En Windows no existe
// soporte nativo -- ver IsSupportedPlatform -- así que ahí GetTokenAsync devuelve null y los
// eventos nunca disparan, en vez de tocar CrossFirebaseCloudMessaging.Current (el paquete solo
// publica una librería de reemplazo vacía para esa plataforma; no la llamamos ni para leer
// Current, por si su propia implementación lanzase al hacerlo).
public class FirebasePushNotificationService : IPushNotificationService
{
    public event EventHandler<string>? TokenChanged;
    public event EventHandler<PushNotificationInfo>? NotificationReceived;
    public event EventHandler<PushNotificationInfo>? NotificationTapped;

    public FirebasePushNotificationService()
    {
        if (!IsSupportedPlatform) return;

        CrossFirebaseCloudMessaging.Current.TokenChanged += (_, args) =>
            TokenChanged?.Invoke(this, args.Token);

        CrossFirebaseCloudMessaging.Current.NotificationReceived += (_, args) =>
            NotificationReceived?.Invoke(this, ToInfo(args.Notification));

        CrossFirebaseCloudMessaging.Current.NotificationTapped += (_, args) =>
            NotificationTapped?.Invoke(this, ToInfo(args.Notification));
    }

    public async Task<string?> GetTokenAsync()
    {
        if (!IsSupportedPlatform) return null;

        try
        {
            return await CrossFirebaseCloudMessaging.Current.GetTokenAsync();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Push] GetTokenAsync falló: {ex.Message}");
            return null;
        }
    }

    private static bool IsSupportedPlatform =>
        DeviceInfo.Platform == DevicePlatform.Android || DeviceInfo.Platform == DevicePlatform.iOS;

    private static PushNotificationInfo ToInfo(FCMNotification notification) => new()
    {
        Title = notification.Title ?? string.Empty,
        Body = notification.Body ?? string.Empty,
        Data = notification.Data ?? new Dictionary<string, string>()
    };
}
