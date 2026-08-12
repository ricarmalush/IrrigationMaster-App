using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.UI.Maui.Common;

namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.ReportIncident;

/// <summary>
/// Reportar Incidencia, visible para los 3 roles sin restricción -- aunque el caso de uso
/// principal sea el Vecino, el backend resuelve el permiso (REPORT_INCIDENT) y los destinatarios
/// (Presidentes de la organización del autor) por su cuenta, así que no hace falta gating aquí.
/// </summary>
public partial class ReportIncidentViewModel : ObservableObject
{
    private readonly INotificationService _notificationService;
    private readonly IAlertService _alertService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy => !IsBusy;

    [ObservableProperty] public partial string Description { get; set; } = string.Empty;

    public ReportIncidentViewModel(INotificationService notificationService, IAlertService alertService)
    {
        _notificationService = notificationService;
        _alertService = alertService;
    }

    [RelayCommand]
    internal async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(Description))
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgMissingIncidentDescription);
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _notificationService.ReportIncidentAsync(Description.Trim());

            if (result.IsSuccess)
            {
                Description = string.Empty;
                await _alertService.ShowAsync(AppStrings.SuccessTitle, AppStrings.IncidentReportedSuccess);
            }
            else
            {
                await _alertService.ShowAsync(AppStrings.ErrorTitle, BuildFailureMessage(result));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error ReportIncident]: {ex.Message}");
            await _alertService.ShowAsync(AppStrings.ErrorTitle, AppStrings.ApiConnectionError);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string BuildFailureMessage(UserActionResult result)
    {
        if (result.Errors is { Count: > 0 })
            return string.Join("\n", result.Errors.Select(e => $"{e.PropertyMessage}: {e.ErrorMessage}"));

        return string.IsNullOrWhiteSpace(result.Message) ? AppStrings.ApiConnectionError : result.Message;
    }
}
