using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Features.Level1_Core.Welcome;

/// <summary>
/// Cerebro de la pantalla de bienvenida animada (primera pantalla que ve el usuario, antes de
/// Login/Home). La orquestación visual (FadeTo/ScaleTo, temporización) vive en el code-behind de
/// WelcomePage -- no es testable sin runtime MAUI y no es lógica de negocio. Lo único que este
/// ViewModel decide, y que sí es testable, es A DÓNDE navegar una vez termina la animación.
/// </summary>
public partial class WelcomeViewModel : ObservableObject
{
    // Mismas rutas absolutas que ya usa LoginViewModel para "Home" (AdminMenuRoute) y que declara
    // AppShell.xaml para Login -- centralizadas aquí como constantes para no repetir el string.
    public const string HomeRoute = "//AdminMenuPage";
    public const string LoginRoute = "//MainPage";

    private readonly ITokenStorage _tokenStorage;
    private readonly INavigationService _navigationService;

    public WelcomeViewModel(ITokenStorage tokenStorage, INavigationService navigationService)
    {
        _tokenStorage = tokenStorage;
        _navigationService = navigationService;
    }

    // Testable sin runtime MAUI: decide el destino según si hay una sesión guardada (mismo
    // criterio que usa ApiService.AttachAuthHeadersAsync para saber si hay token). No valida si
    // el JWT sigue vigente -- igual que el resto de la App, que descubre un token caducado al
    // primer 401 real, no de antemano.
    internal async Task<string> DetermineDestinationRouteAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();
        return string.IsNullOrWhiteSpace(token) ? LoginRoute : HomeRoute;
    }

    [RelayCommand]
    public async Task NavigateToDestinationAsync()
    {
        var route = await DetermineDestinationRouteAsync();
        await _navigationService.GoToAsync(route);
    }
}
