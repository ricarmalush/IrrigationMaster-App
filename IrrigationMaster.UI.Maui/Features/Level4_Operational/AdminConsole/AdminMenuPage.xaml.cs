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
                await Navigation.PushModalAsync(userManagementPage);
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
    /// SECCIÓN 1: MONITOR DE CAMPO
    /// </summary>
    private async void OnLiveStatusClicked(object sender, EventArgs e)
    {
        await DisplayAlert(AppStrings.ConsoleTitle, AppStrings.LiveStatusLoading, "OK");
    }

    private async void OnManageQueuesClicked(object sender, EventArgs e)
    {
        await DisplayAlert(AppStrings.ConsoleTitle, AppStrings.ManageQueuesLoading, "OK");
    }

    /// <summary>
    /// SECCIÓN 2: CONFIGURACIÓN Y ALERTAS
    /// </summary>
    private async void OnCaudalConfigClicked(object sender, EventArgs e)
    {
        await DisplayAlert(AppStrings.ConsoleTitle, AppStrings.CaudalConfigLoading, "OK");
    }

    private async void OnPublishIncidentsClicked(object sender, EventArgs e)
    {
        await DisplayAlert(AppStrings.ConsoleTitle, AppStrings.PublishIncidentsLoading, "OK");
    }

    /// <summary>
    /// SECCIÓN 3: HISTORIAL Y MANTENIMIENTO
    /// </summary>
    private async void OnHistoryLogClicked(object sender, EventArgs e)
    {
        await DisplayAlert(AppStrings.ConsoleTitle, AppStrings.HistoryLogLoading, "OK");
    }

    private async void OnInfrastructureClicked(object sender, EventArgs e)
    {
        await DisplayAlert(AppStrings.ConsoleTitle, AppStrings.InfrastructureLoading, "OK");
    }

    /// <summary>
    /// SECCIÓN 4: AJUSTES DE LA APLICACIÓN
    /// </summary>
    private async void OnSettingsButtonClicked(object sender, EventArgs e)
    {
        try
        {
            var settingsPage = Handler?.MauiContext?.Services.GetService<SystemSettingsPage>();

            if (settingsPage != null)
            {
                // 🟢 EL CAMBIO ESTÁ AQUÍ: Usamos PushModalAsync
                await Navigation.PushModalAsync(settingsPage);
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