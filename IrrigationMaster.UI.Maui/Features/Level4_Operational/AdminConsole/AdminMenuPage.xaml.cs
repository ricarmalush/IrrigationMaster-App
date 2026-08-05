using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level3_Functional.Users;

namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.AdminConsole;

public partial class AdminMenuPage : ContentPage
{
    // La App todavía no lee permisos granulares del JWT (pendiente aparte): por ahora, cualquier
    // usuario autenticado que no sea VECINO puede ver el botón. El backend es quien realmente
    // decide si la acción concreta (aprobar/asignar andador/cambiar rol) está permitida.
    private const string VecinoRoleCode = "VECINO";

    private readonly ICurrentSession _currentSession;

    public AdminMenuPage(ICurrentSession currentSession)
    {
        InitializeComponent();
        _currentSession = currentSession;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var role = await _currentSession.GetRoleAsync();
        UserManagementButton.IsVisible = !string.Equals(role, VecinoRoleCode, StringComparison.OrdinalIgnoreCase);
    }

    private async void OnUserManagementClicked(object sender, EventArgs e)
    {
        try
        {
            var userManagementPage = Handler?.MauiContext?.Services.GetService<UserManagementPage>();

            if (userManagementPage != null)
            {
                // PushAsync (no PushModalAsync): así queda en la misma pila de navegación jerárquica
                // que AdminMenuPage y Shell le añade automáticamente la flecha de retroceso estándar.
                // Un push modal, en cambio, sale de esa pila y no recibe esa flecha.
                await Navigation.PushAsync(userManagementPage);
            }
            else
            {
                await DisplayAlert(AppStrings.SystemErrorTitle, "No se pudo cargar la gestión de usuarios.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Navigation Error]: {ex.Message}");
        }
    }

    /// <summary>
    /// SECCIÓN: AJUSTES DE LA APLICACIÓN
    /// </summary>
    private async void OnSettingsButtonClicked(object sender, EventArgs e)
    {
        try
        {
            var settingsPage = Handler?.MauiContext?.Services.GetService<SystemSettingsPage>();

            if (settingsPage != null)
            {
                // PushModalAsync (no PushAsync) -- a diferencia de OnUserManagementClicked, aquí SÍ
                // hace falta: SystemSettingsPage es un TabbedPage, y empujar un TabbedPage con
                // Navigation.PushAsync sobre la pila de un NavigationPage es un bug conocido de
                // Android (el ViewPager/FragmentManager nativo del TabbedPage entra en conflicto
                // con las transiciones de fragments del NavigationPage que lo aloja), que revienta
                // con IllegalArgumentException: 'No view found for id ... navigationlayout_toptabs'
                // -- reproducido en dispositivo real, independientemente de cómo se gestionen sus
                // Children (probado tanto quitando pestañas como añadiendo solo las necesarias).
                // PushModalAsync evita el conflicto por completo al no anidar el TabbedPage dentro
                // de la navegación jerárquica del NavigationPage.
                //
                // Se envuelve en un NavigationPage propio (no compartido con la pila de
                // AdminMenuPage -- eso sería volver al bug de arriba) porque un TabbedPage
                // modal "desnudo" no renderiza NINGUNA barra de acción en Android: el
                // ToolbarItem de "← Atrás" de SystemSettingsPage no tenía dónde pintarse
                // (confirmado en dispositivo real). Como raíz de un NavigationPage recién
                // creado sí obtiene esa barra, sin reintroducir el conflicto original porque
                // esta pila nace vacía y nunca comparte Navigation con AdminMenuPage.
                await Navigation.PushModalAsync(new NavigationPage(settingsPage));
            }
            else
            {
                await DisplayAlert(AppStrings.SystemErrorTitle, "No se pudo cargar la interfaz de configuración.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Navigation Error]: {ex.Message}");
        }
    }

    /// <summary>
    /// CIERRE DE SEGURIDAD
    /// </summary>
    private async void OnCloseConsoleClicked(object sender, EventArgs e)
    {
        await Navigation.PopAsync();
    }
}