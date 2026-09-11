using CommunityToolkit.Mvvm.ComponentModel;
using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.UI.Maui.Common;
using System.Windows.Input;

namespace IrrigationMaster.UI.Maui.Features.Level1_Core.Login;

// ObservableObject (CommunityToolkit.Mvvm), estándar de la App: ver SystemSettingsViewModel.
public partial class LoginViewModel : ObservableObject
{
    private readonly IAuthService _authService;
    private readonly ICurrentSession _currentSession;
    private readonly IAlertService _alertService;
    private readonly INavigationService _navigationService;
    private readonly IPushNotificationService _pushNotificationService;
    private readonly IUserDeviceService _userDeviceService;

    // Rutas de navegación centralizadas en constantes
    private const string AdminMenuRoute = "//AdminMenuPage";
    private const string RegisterRoute = "RegisterPage";

    [ObservableProperty] public partial string Email { get; set; } = string.Empty;
    [ObservableProperty] public partial string Password { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    public partial bool IsLoading { get; set; }

    public bool IsNotLoading => !IsLoading;

    public ICommand LoginCommand { get; }
    public ICommand NavigateToRegisterCommand { get; }
    public ICommand NavigateToSettingsCommand { get; }

    public LoginViewModel(
        IAuthService authService,
        ICurrentSession currentSession,
        IAlertService alertService,
        INavigationService navigationService,
        IPushNotificationService pushNotificationService,
        IUserDeviceService userDeviceService)
    {
        _authService = authService;
        _currentSession = currentSession;
        _alertService = alertService;
        _navigationService = navigationService;
        _pushNotificationService = pushNotificationService;
        _userDeviceService = userDeviceService;

        LoginCommand = new Command(async () => await ExecuteLoginAsync());
        NavigateToRegisterCommand = new Command(async () => await ExecuteNavigateToRegisterAsync());
        NavigateToSettingsCommand = new Command(async () => await _navigationService.GoToAsync(AdminMenuRoute));
    }

    internal async Task ExecuteLoginAsync()
    {
        // 1. Validación previa en el cliente
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.LoginValidationWarning);
            return;
        }

        IsLoading = true;

        // 2. Petición asíncrona a la infraestructura
        var loginResult = await _authService.LoginAsync(Email.Trim(), Password);

        IsLoading = false;

        // 3. LÓGICA PURA Y PROFESIONAL: Confiamos ciegamente en el contrato de la API
        if (loginResult?.IsSuccess == true)
        {
            // GUARDAMOS LA SESIÓN COMPLETA (token + organización + rol)
            if (loginResult.Data != null)
            {
                await _currentSession.EstablishAsync(loginResult.Data.ToString()!);
            }

            // Fire-and-forget a propósito: el registro del token push no debe bloquear la
            // navegación ni fallar visible al usuario -- si falla, PushNotificationCoordinator
            // reintentará en el próximo TokenChanged. Se deja rastro en el log de depuración para
            // poder diagnosticar en caliente sin molestar al usuario.
            _ = RegisterDeviceForPushAsync();

            await _navigationService.GoToAsync(AdminMenuRoute);
        }
        else
        {
            // El inicio de sesión falló. Pintamos el mensaje que viene del backend o el de respaldo de red.
            string errorMsg = loginResult?.Message ?? AppStrings.NetworkFallbackError;

            // Sin licencia activa (402) y cuenta desactivada por un admin (403) son casos
            // distintos del fallo de credenciales genérico -- cada uno con su propio título,
            // señalizados por ApiService a partir del status code, no comparando el mensaje.
            string title = loginResult?.IsLicenceError == true ? AppStrings.LicenceRequiredTitle
                : loginResult?.IsAccountDeactivated == true ? AppStrings.AccountDeactivatedTitle
                : AppStrings.ErrorTitle;
            await _alertService.ShowAsync(title, errorMsg);
        }
    }

    internal async Task ExecuteNavigateToRegisterAsync()
    {
        await _navigationService.GoToAsync(RegisterRoute);
    }

    // Separado de ExecuteLoginAsync para poder testearlo aparte y para que quede claro que es
    // fire-and-forget: nunca lanza hacia el llamador, cualquier fallo (sin token, sin red, backend
    // caído) solo se refleja en el log de depuración.
    internal async Task RegisterDeviceForPushAsync()
    {
        try
        {
            var token = await _pushNotificationService.GetTokenAsync();
            if (string.IsNullOrWhiteSpace(token))
            {
                System.Diagnostics.Debug.WriteLine("[Push] Sin token disponible tras login (plataforma sin soporte o Firebase aún no lo entregó).");
                return;
            }

            var result = await _userDeviceService.RegisterDeviceAsync(token, DeviceInfo.Model, $"{DeviceInfo.Platform} {DeviceInfo.VersionString}");
            System.Diagnostics.Debug.WriteLine(result.IsSuccess
                ? $"[Push] Dispositivo registrado correctamente (Id={result.Data})."
                : $"[Push] Fallo al registrar dispositivo: {result.Message}");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Push] Excepción registrando dispositivo: {ex.Message}");
        }
    }
}
