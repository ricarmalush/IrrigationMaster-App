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
                // PushAsync (no PushModalAsync): mismo arreglo que en OnUserManagementClicked --
                // así queda en la misma pila de navegación jerárquica que AdminMenuPage y Shell
                // le añade automáticamente la flecha de retroceso estándar, en el mismo sitio y
                // con el mismo aspecto que en el resto de la App.
                //
                // Esto solo es seguro en Android porque SystemSettingsPage dejó de ser un
                // TabbedPage nativo: empujar un TabbedPage con PushAsync sobre esta misma pila
                // revienta con IllegalArgumentException ('No view found for id ...
                // navigationlayout_toptabs') -- bug de plataforma reproducido en dispositivo
                // real, independientemente de cómo se gestionaran sus Children. Ahora es un
                // ContentPage normal con una tira de pestañas hecha a mano (ver
                // SystemSettingsPage.xaml.cs), así que este PushAsync es idéntico al de
                // OnUserManagementClicked y no tiene ese problema.
                await Navigation.PushAsync(settingsPage);
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