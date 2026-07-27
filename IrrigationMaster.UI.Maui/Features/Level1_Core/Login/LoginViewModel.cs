using IrrigationMaster.Mobile.Core.Interfaces;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level1_Core.Register;
using System.Diagnostics;
using System.Windows.Input;

namespace IrrigationMaster.UI.Maui.Features.Level1_Core.Login;

public partial class LoginViewModel : BindableObject
{
    private readonly IAuthService _authService;
    private readonly ITokenStorage _tokenStorage;
    private string _email = string.Empty;
    private string _password = string.Empty;
    private bool _isLoading;

    // Rutas de navegación centralizadas en constantes
    private const string AdminMenuRoute = "//AdminMenuPage";
    private const string RegisterRoute = "RegisterPage";

    public string Email
    {
        get => _email;
        set { _email = value; OnPropertyChanged(); }
    }

    public string Password
    {
        get => _password;
        set { _password = value; OnPropertyChanged(); }
    }

    public bool IsLoading
    {
        get => _isLoading;
        set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotLoading)); }
    }

    public bool IsNotLoading => !IsLoading;

    public ICommand LoginCommand { get; }
    public ICommand NavigateToRegisterCommand { get; }
    public ICommand NavigateToSettingsCommand { get; }

    public LoginViewModel(IAuthService authService, ITokenStorage tokenStorage)
    {
        _authService = authService;
        _tokenStorage = tokenStorage;

        LoginCommand = new Command(async () => await ExecuteLoginAsync());
        NavigateToRegisterCommand = new Command(async () => await ExecuteNavigateToRegisterAsync());
        NavigateToSettingsCommand = new Command(async () => await Shell.Current.GoToAsync(AdminMenuRoute));
    }

    private async Task ExecuteLoginAsync()
    {
        // 1. Validación previa en el cliente
        if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await Shell.Current.DisplayAlert(AppStrings.AttentionTitle, AppStrings.LoginValidationWarning, "OK");
            return;
        }

        IsLoading = true;

        // 2. Petición asíncrona a la infraestructura
        var loginResult = await _authService.LoginAsync(Email.Trim(), Password);

        IsLoading = false;

        // 3. LÓGICA PURA Y PROFESIONAL: Confiamos ciegamente en el contrato de la API
        if (loginResult?.IsSuccess == true)
        {
            try
            {
                // GUARDAMOS LA SESIÓN COMPLETA (token + organización + rol)
                if (loginResult.Data != null)
                {
                    var jwtToken = loginResult.Data.ToString()!;
                    var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                    var parsedToken = handler.ReadJwtToken(jwtToken);

                    var organizationId = parsedToken.Claims.FirstOrDefault(c => c.Type == "organizationId")?.Value ?? string.Empty;
                    var role = parsedToken.Claims.FirstOrDefault(c => c.Type == System.Security.Claims.ClaimTypes.Role)?.Value ?? string.Empty;

                    await _tokenStorage.SaveSessionAsync(jwtToken, organizationId, role);
                }

                // Intentamos la navegación limpia por Shell
                await Shell.Current.GoToAsync(AdminMenuRoute);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[Shell Navigation Error]: {ex.Message}");

                // PLAN DE RESPALDO: Navegación jerárquica si el Shell falla
                if (Shell.Current.CurrentPage != null)
                {
                    await Shell.Current.CurrentPage.Navigation.PushAsync(
                        new Features.Level4_Operational.AdminConsole.AdminMenuPage()
                    );
                }
                else
                {
                    await Shell.Current.DisplayAlert(AppStrings.SystemErrorTitle, AppStrings.NavigationFallbackError, "OK");
                }
            }
        }
        else
        {
            // El inicio de sesión falló. Pintamos el mensaje que viene del backend o el de respaldo de red.
            string errorMsg = loginResult?.Message ?? AppStrings.NetworkFallbackError;
            await Shell.Current.DisplayAlert(AppStrings.ErrorTitle, errorMsg, "OK");
        }
    }

    private async Task ExecuteNavigateToRegisterAsync()
    {
        try
        {
            // 1. Intentamos la navegación limpia por el Shell de MAUI
            await Shell.Current.GoToAsync(RegisterRoute);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Register Navigation Error]: {ex.Message}");

            if (Shell.Current.CurrentPage != null)
            {
                // 2. SOLUCIÓN MODERNA: Extraemos el contenedor de servicios
                var services = Shell.Current.Handler?.MauiContext?.Services;
                var registerViewModel = services?.GetService<RegisterViewModel>();

                if (registerViewModel != null)
                {
                    await Shell.Current.CurrentPage.Navigation.PushAsync(new RegisterPage(registerViewModel));
                }
                else
                {
                    try
                    {
                        // 3. PLAN DE RESCATE MÁXIMO
                        if (Activator.CreateInstance<RegisterViewModel>() is RegisterViewModel fallbackVm)
                        {
                            await Shell.Current.CurrentPage.Navigation.PushAsync(new RegisterPage(fallbackVm));
                        }
                        else
                        {
                            await Shell.Current.DisplayAlert(AppStrings.SystemErrorTitle, AppStrings.NavigationFallbackError, "OK");
                        }
                    }
                    catch (Exception innerEx)
                    {
                        Debug.WriteLine($"[Fallback Instantiation Error]: {innerEx.Message}");
                        await Shell.Current.DisplayAlert(AppStrings.SystemErrorTitle, AppStrings.NavigationFallbackError, "OK");
                    }
                }
            }
        }
    }
}