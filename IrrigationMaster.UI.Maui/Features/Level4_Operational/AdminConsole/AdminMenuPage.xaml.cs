using IrrigationMaster.UI.Maui.Common; 

namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.AdminConsole;

public partial class AdminMenuPage : ContentPage
{
    public AdminMenuPage()
    {
        InitializeComponent();
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