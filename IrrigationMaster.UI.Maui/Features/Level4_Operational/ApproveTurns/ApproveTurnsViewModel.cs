using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.UI.Maui.Common;
using System.Collections.ObjectModel;

namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.ApproveTurns;

// Un turno de riego en Requested, a la espera de aprobación. Plantilla plana (no ObservableObject):
// se reconstruye entera en cada LoadAsync, igual que NotificationItem/NeighborStatusItem.
public class PendingTurnItem
{
    public Guid Id { get; init; }
    public string RequesterFullName { get; init; } = string.Empty;
    public DateTime ScheduledStart { get; init; }
    public DateTime ScheduledEnd { get; init; }
    public int? HouseNumber { get; init; }

    public string HouseNumberDisplay => HouseNumber?.ToString() ?? "—";

    public string ScheduleDisplay =>
        $"{ScheduledStart.ToLocalTime():dd/MM/yyyy HH:mm} - {ScheduledEnd.ToLocalTime():HH:mm}";
}

// Un andador con al menos un turno pendiente, ya ordenado por prioridad (HouseNumber descendente,
// ThenBy hora de solicitud) tal como lo devuelve el backend -- mismo patrón anidado que
// WalkwayStatusItem/NeighborStatusItem en IrrigationStatusViewModel (vista hermana).
public class WalkwayApprovalGroupItem
{
    public Guid WalkwayId { get; init; }
    public string WalkwayCode { get; init; } = string.Empty;
    public List<PendingTurnItem> Turns { get; init; } = [];
}

/// <summary>
/// Turnos de riego pendientes de aprobar de la organización del usuario logueado, agrupados por
/// andador. Visible solo para Presidente/Vicepresidente/SUPERADMIN (mismo gating que
/// CommunityBroadcastButton en AdminMenuPage) -- el backend exige el permiso TURN_APPROVE o rol
/// SUPERADMIN para GetPendingApprovalTurnsAsync/ApproveTurnAsync, esta pantalla no duplica esa
/// comprobación.
/// </summary>
public partial class ApproveTurnsViewModel : ObservableObject
{
    private readonly IIrrigationService _irrigationService;
    private readonly IAlertService _alertService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<WalkwayApprovalGroupItem> Groups { get; } = [];

    public ApproveTurnsViewModel(IIrrigationService irrigationService, IAlertService alertService)
    {
        _irrigationService = irrigationService;
        _alertService = alertService;
    }

    [RelayCommand]
    internal async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var groups = await _irrigationService.GetPendingApprovalTurnsAsync();

            Groups.Clear();
            foreach (var group in groups ?? [])
            {
                Groups.Add(new WalkwayApprovalGroupItem
                {
                    WalkwayId = group.WalkwayId,
                    WalkwayCode = group.WalkwayCode,
                    Turns = group.Turns.Select(turn => new PendingTurnItem
                    {
                        Id = turn.Id,
                        RequesterFullName = turn.RequesterFullName,
                        ScheduledStart = turn.ScheduledStart,
                        ScheduledEnd = turn.ScheduledEnd,
                        HouseNumber = turn.HouseNumber
                    }).ToList()
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Loading PendingTurns]: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    internal async Task ApproveAsync(PendingTurnItem? turn)
    {
        if (turn is null) return;

        var result = await _irrigationService.ApproveTurnAsync(turn.Id);
        if (result.IsSuccess)
        {
            await _alertService.ShowAsync(AppStrings.SuccessTitle, AppStrings.TurnApprovedSuccess);
            await LoadAsync();
        }
        else
        {
            await _alertService.ShowAsync(AppStrings.ErrorTitle, BuildFailureMessage(result));
        }
    }

    private static string BuildFailureMessage(UserActionResult result)
    {
        if (result.Errors is { Count: > 0 })
            return string.Join("\n", result.Errors.Select(e => $"{e.PropertyMessage}: {e.ErrorMessage}"));

        return string.IsNullOrWhiteSpace(result.Message) ? AppStrings.ApiConnectionError : result.Message;
    }
}
