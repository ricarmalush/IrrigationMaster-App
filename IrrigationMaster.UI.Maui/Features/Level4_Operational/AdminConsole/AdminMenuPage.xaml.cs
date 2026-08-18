using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level3_Functional.Users;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.ApproveTurns;
using IrrigationMaster.UI.Maui.Features.Level3_Functional.IrrigationPrograms;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.CommunityBroadcast;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.IrrigationStatus;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.Notifications;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.ReportIncident;

namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.AdminConsole;

public partial class AdminMenuPage : ContentPage
{
    // La App todavía no lee permisos granulares del JWT (pendiente aparte): CachedRole/GetRoleAsync
    // solo traen un único código de rol, nunca la lista de permisos concedidos. Por eso el gating
    // real sigue siendo por código de rol -- mismo criterio que SystemSettingsViewModel.
    // ApplyTabVisibility -- pero ENUMERANDO qué roles concretos ven cada botón, en vez de un único
    // flag "no es Vecino" (isNotVecino): con 4 roles no-Vecino que ya no comparten exactamente las
    // mismas acciones (Coordinador de Riego ve Avisos y Calendario, pero no Gestión de Usuarios ni
    // Aprobar Turnos), ese flag único dejó de alcanzar.
    private const string SuperAdminRoleCode = "SUPERADMIN";
    private const string PresidenteRoleCode = "PRESIDENTE";
    private const string VicepresidenteRoleCode = "VICEPRESIDENTE";
    private const string CoordinadorRiegoRoleCode = "COORDINADOR_RIEGO";

    private readonly ICurrentSession _currentSession;
    private readonly IStructureService _structureService;

    public AdminMenuPage(ICurrentSession currentSession, IStructureService structureService)
    {
        InitializeComponent();
        _currentSession = currentSession;
        _structureService = structureService;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var role = await _currentSession.GetRoleAsync();
        var visibility = ComputeMenuVisibility(role);

        UserManagementButton.IsVisible = visibility.ShowUserManagement;
        CommunityBroadcastButton.IsVisible = visibility.ShowCommunityBroadcast;
        ApproveTurnsButton.IsVisible = visibility.ShowApproveTurns;
        IrrigationProgramsButton.IsVisible = visibility.ShowIrrigationPrograms;

        // Estas dos etiquetas de sección agrupan un único botón cada una: deben ocultarse junto
        // con SU botón (no con un flag compartido) para no quedar huérfanas sin nada debajo.
        HistorialLabel.IsVisible = visibility.ShowUserManagement;
        AvisosLabel.IsVisible = visibility.ShowCommunityBroadcast;

        await LoadOrganizationNameAsync();
    }

    // Estático y testable a propósito (mismo motivo que BuildHeaderText): así el matriz de
    // visibilidad por rol se puede cubrir con tests reales sin necesitar un ContentPage con
    // Handler de MAUI corriendo.
    internal static (bool ShowUserManagement, bool ShowCommunityBroadcast, bool ShowApproveTurns, bool ShowIrrigationPrograms) ComputeMenuVisibility(string? role)
    {
        bool isSuperAdmin = string.Equals(role, SuperAdminRoleCode, StringComparison.OrdinalIgnoreCase);
        bool isPresidente = string.Equals(role, PresidenteRoleCode, StringComparison.OrdinalIgnoreCase);
        bool isVicepresidente = string.Equals(role, VicepresidenteRoleCode, StringComparison.OrdinalIgnoreCase);
        bool isCoordinadorRiego = string.Equals(role, CoordinadorRiegoRoleCode, StringComparison.OrdinalIgnoreCase);

        return (
            // Gestión de Usuarios y Roles / Aprobar Turnos: sin Coordinador de Riego -- solo la
            // autoridad de organización (Presidente/Vicepresidente) y SUPERADMIN.
            ShowUserManagement: isSuperAdmin || isPresidente || isVicepresidente,
            // Avisar a mi comunidad: Presidente/Vicepresidente/Coordinador de Riego/SUPERADMIN.
            ShowCommunityBroadcast: isSuperAdmin || isPresidente || isVicepresidente || isCoordinadorRiego,
            ShowApproveTurns: isSuperAdmin || isPresidente || isVicepresidente,
            // Calendario de Riego: antes exclusivo de SUPERADMIN: ahora también Coordinador de
            // Riego, el rol pensado precisamente para esto (permiso MANAGE_IRRIGATION_PROGRAMS).
            ShowIrrigationPrograms: isSuperAdmin || isCoordinadorRiego
        );
    }

    // Mismo endpoint que ya usa "Mi Organización" en SystemSettingsViewModel
    // (LoadMyOrganizationAsync) -- se lee tras el login, nunca un nombre fijo en el XAML, para que
    // cada organización vea el suyo propio en este mismo APK multi-tenant.
    private async Task LoadOrganizationNameAsync()
    {
        try
        {
            var organizationIdRaw = await _currentSession.GetOrganizationIdAsync();
            if (!Guid.TryParse(organizationIdRaw, out var organizationId)) return;

            var organization = await _structureService.GetOrganizationAsync(organizationId);
            HeaderTitleLabel.Text = BuildHeaderText(organization?.Name);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Loading OrganizationName]: {ex.Message}");
        }
    }

    // "Gestión Operativa" a secas si el nombre no llegó a cargar (fallo de red, aún no
    // resuelto) -- nunca debe volver a mostrarse un nombre de cliente fijo.
    internal static string BuildHeaderText(string? organizationName) =>
        string.IsNullOrWhiteSpace(organizationName)
            ? "Gestión Operativa"
            : $"Gestión Operativa - {organizationName}";

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
    /// SECCIÓN: RIEGO -- Calendario de Riego, visible para SUPERADMIN/Coordinador de Riego
    /// (permiso MANAGE_IRRIGATION_PROGRAMS en el backend).
    /// </summary>
    private async void OnIrrigationProgramsClicked(object sender, EventArgs e)
    {
        try
        {
            var irrigationProgramsPage = Handler?.MauiContext?.Services.GetService<IrrigationProgramsPage>();

            if (irrigationProgramsPage != null)
            {
                // PushAsync (no PushModalAsync): mismo motivo que OnUserManagementClicked.
                await Navigation.PushAsync(irrigationProgramsPage);
            }
            else
            {
                await DisplayAlert(AppStrings.SystemErrorTitle, "No se pudo cargar el calendario de riego.", "OK");
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