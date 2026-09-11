using CommunityToolkit.Maui.Alerts;
using IrrigationMaster.Mobile.Application.Features.Models.Devices;
using IrrigationMaster.Mobile.Application.Interfaces;
#if ANDROID
using Android.Media;
#endif

namespace IrrigationMaster.UI.Maui.Services;

// Orquestador de vida completa del push, un único singleton resuelto una sola vez al arrancar la
// App (ver MauiProgram) para que sus suscripciones queden activas todo el tiempo -- distinto del
// registro puntual que dispara LoginViewModel justo tras iniciar sesión (ese cubre "login nuevo";
// este cubre "el token cambió mientras la sesión ya estaba abierta", que puede pasar en cualquier
// momento) y de tocar una notificación con la App en segundo plano/cerrada.
public class PushNotificationCoordinator
{
    private readonly IUserDeviceService _userDeviceService;
    private readonly ITokenStorage _tokenStorage;
    private readonly INavigationService _navigationService;

    public PushNotificationCoordinator(
        IPushNotificationService pushNotificationService,
        IUserDeviceService userDeviceService,
        ITokenStorage tokenStorage,
        INavigationService navigationService)
    {
        _userDeviceService = userDeviceService;
        _tokenStorage = tokenStorage;
        _navigationService = navigationService;

        pushNotificationService.TokenChanged += OnTokenChanged;
        pushNotificationService.NotificationTapped += OnNotificationTapped;
        pushNotificationService.NotificationReceived += OnNotificationReceivedInForeground;
    }

    private async void OnTokenChanged(object? sender, string token)
    {
        // Un refresco de token sin sesión activa no tiene a quién asociarlo en el backend (que
        // resuelve el dueño del dispositivo desde el JWT) -- se reenviará solo al siguiente login.
        var jwt = await _tokenStorage.GetTokenAsync();
        if (string.IsNullOrWhiteSpace(jwt))
        {
            System.Diagnostics.Debug.WriteLine("[Push] TokenChanged sin sesión activa: no se reenvía.");
            return;
        }

        var result = await _userDeviceService.RegisterDeviceAsync(token, DeviceInfo.Model, $"{DeviceInfo.Platform} {DeviceInfo.VersionString}");
        System.Diagnostics.Debug.WriteLine(result.IsSuccess
            ? "[Push] Token refrescado y reenviado correctamente."
            : $"[Push] Fallo al reenviar el token refrescado: {result.Message}");
    }

    private async void OnNotificationTapped(object? sender, PushNotificationInfo notification)
    {
        var route = DeepLinkRouteResolver.Resolve(notification);
        if (route is null) return;

        try
        {
            await _navigationService.GoToAsync(route);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Push] No se pudo navegar a la ruta '{route}': {ex.Message}");
        }
    }

    // App en primer plano: NO se muestra la notificación nativa del sistema (ver
    // FCMNotification.IsSilentInForeground en Plugin.Firebase), así que la única señal para el
    // usuario es este banner ligero in-app -- nunca una notificación de sistema duplicada. El
    // Snackbar en sí no trae sonido (ningún snackbar/toast de Material Design lo lleva, no es un
    // fallo nuestro): sin sonido, un banner momentáneo pasa fácilmente desapercibido, así que se
    // reproduce aparte el sonido de notificación por defecto del propio dispositivo.
    private static async void OnNotificationReceivedInForeground(object? sender, PushNotificationInfo notification)
    {
        PlayNotificationSound();

        try
        {
            var snackbar = new Snackbar
            {
                Text = string.IsNullOrWhiteSpace(notification.Body) ? notification.Title : $"{notification.Title}: {notification.Body}",
                Duration = TimeSpan.FromSeconds(4)
            };

            await snackbar.Show();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Push] No se pudo mostrar el banner in-app: {ex.GetType().Name}: {ex.Message}");
        }
    }

    private static void PlayNotificationSound()
    {
#if ANDROID
        try
        {
            var uri = RingtoneManager.GetDefaultUri(RingtoneType.Notification);
            var ringtone = RingtoneManager.GetRingtone(Android.App.Application.Context, uri);
            ringtone?.Play();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Push] No se pudo reproducir el sonido de notificación: {ex.Message}");
        }
#endif
    }
}
