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

    public LoginViewModel(IAuthService authService, ICurrentSession currentSession, IAlertService alertService, INavigationService navigationService)
    {
        _authService = authService;
        _currentSession = currentSession;
        _alertService = alertService;
        _navigationService = navigationService;

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

            await _navigationService.GoToAsync(AdminMenuRoute);
        }
        else
        {
            // El inicio de sesión falló. Pintamos el mensaje que viene del backend o el de respaldo de red.
            string errorMsg = loginResult?.Message ?? AppStrings.NetworkFallbackError;
            await _alertService.ShowAsync(AppStrings.ErrorTitle, errorMsg);
        }
    }

    internal async Task ExecuteNavigateToRegisterAsync()
    {
        await _navigationService.GoToAsync(RegisterRoute);
    }
}
