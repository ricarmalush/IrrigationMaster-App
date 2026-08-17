using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level3_Functional.Users;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.ApproveTurns;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.CommunityBroadcast;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.IrrigationStatus;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.Notifications;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.ReportIncident;

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
        var isNotVecino = !string.Equals(role, VecinoRoleCode, StringComparison.OrdinalIgnoreCase);
        UserManagementButton.IsVisible = isNotVecino;
        CommunityBroadcastButton.IsVisible = isNotVecino;
        ApproveTurnsButton.IsVisible = isNotVecino;
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
    /// SECCIÓN: RIEGO -- sin gating de rol, visible para los 3 (SUPERADMIN/Presidente/Vecino).
    /// </summary>
    private async void OnIrrigationStatusClicked(object sender, EventArgs e)
    {
        try
        {
            var irrigationStatusPage = Handler?.MauiContext?.Services.GetService<IrrigationStatusPage>();

            if (irrigationStatusPage != null)
            {
                // PushAsync (no PushModalAsync): mismo motivo que OnUserManagementClicked.
                await Navigation.PushAsync(irrigationStatusPage);
            }
            else
            {
                await DisplayAlert(AppStrings.SystemErrorTitle, "No se pudo cargar el estado de riego.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Navigation Error]: {ex.Message}");
        }
    }

    /// <summary>
    /// SECCIÓN: RIEGO -- Aprobar Turnos, visible solo para Presidente/Vicepresidente/SUPERADMIN
    /// (mismo gating que OnCommunityBroadcastClicked).
    /// </summary>
    private async void OnApproveTurnsClicked(object sender, EventArgs e)
    {
        try
        {
            var approveTurnsPage = Handler?.MauiContext?.Services.GetService<ApproveTurnsPage>();

            if (approveTurnsPage != null)
            {
                // PushAsync (no PushModalAsync): mismo motivo que OnUserManagementClicked.
                await Navigation.PushAsync(approveTurnsPage);
            }
            else
            {
                await DisplayAlert(AppStrings.SystemErrorTitle, "No se pudo cargar la aprobación de turnos.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Navigation Error]: {ex.Message}");
        }
    }

    /// <summary>
    /// SECCIÓN: NOTIFICACIONES -- sin gating de rol, visible para los 3 (SUPERADMIN/Presidente/Vecino).
    /// </summary>
    private async void OnNotificationsClicked(object sender, EventArgs e)
    {
        try
        {
            var notificationsPage = Handler?.MauiContext?.Services.GetService<NotificationsPage>();

            if (notificationsPage != null)
            {
                // PushAsync (no PushModalAsync): mismo motivo que OnUserManagementClicked.
                await Navigation.PushAsync(notificationsPage);
            }
            else
            {
                await DisplayAlert(AppStrings.SystemErrorTitle, "No se pudo cargar las notificaciones.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Navigation Error]: {ex.Message}");
        }
    }

    /// <summary>
    /// SECCIÓN: INCIDENCIAS -- sin gating de rol, visible para los 3 (SUPERADMIN/Presidente/Vecino).
    /// </summary>
    private async void OnReportIncidentClicked(object sender, EventArgs e)
    {
        try
        {
            var reportIncidentPage = Handler?.MauiContext?.Services.GetService<ReportIncidentPage>();

            if (reportIncidentPage != null)
            {
                // PushAsync (no PushModalAsync): mismo motivo que OnUserManagementClicked.
                await Navigation.PushAsync(reportIncidentPage);
            }
            else
            {
                await DisplayAlert(AppStrings.SystemErrorTitle, "No se pudo cargar el formulario de incidencias.", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Navigation Error]: {ex.Message}");
        }
    }

    /// <summary>
    /// SECCIÓN: AVISOS -- visible solo para Presidente/SUPERADMIN (mismo gating que
    /// UserManagementButton). Un Vecino solo puede reportar incidencias individuales al
    /// Presidente, no enviar avisos masivos a su comunidad.
    /// </summary>
    private async void OnCommunityBroadcastClicked(object sender, EventArgs e)
    {
        try
        {
            var communityBroadcastPage = Handler?.MauiContext?.Services.GetService<CommunityBroadcastPage>();

            if (communityBroadcastPage != null)
            {
                // PushAsync (no PushModalAsync): mismo motivo que OnUserManagementClicked.
                await Navigation.PushAsync(communityBroadcastPage);
            }
            else
            {
                await DisplayAlert(AppStrings.SystemErrorTitle, "No se pudo cargar el formulario de avisos.", "OK");
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