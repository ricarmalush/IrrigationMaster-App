using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.UI.Maui.Common;
using System.Collections.ObjectModel;

namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.IrrigationStatus;

// Una fila "mi vecino" dentro de un andador. Plantilla plana (no ObservableObject): se reconstruye
// entera en cada LoadAsync, no hay estado mutable propio que necesite notificar cambios (a
// diferencia de UserListItem, que sí tiene un Picker con selección pendiente de confirmar).
public class NeighborStatusItem
{
    public Guid UserId { get; init; }
    public Guid TurnId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string RawStatus { get; init; } = string.Empty;
    public string StatusDisplay { get; init; } = string.Empty;

    // "¿Es esta la fila del usuario logueado?" -- comparación de conveniencia de UI: el backend ya
    // bloquea a nivel de negocio que alguien actúe sobre el turno de otro vecino.
    public bool IsMine { get; init; }

    public bool ShowStartButton => IsMine && RawStatus == IrrigationStatusViewModel.WaitingStatus;
    public bool ShowCompleteButton => IsMine && RawStatus == IrrigationStatusViewModel.WateringStatus;
}

public class WalkwayStatusItem
{
    public Guid WalkwayId { get; init; }
    public string WalkwayCode { get; init; } = string.Empty;
    public List<NeighborStatusItem> Neighbors { get; init; } = [];

    public bool HasNeighbors => Neighbors.Count > 0;
    public bool ShowEmptyState => !HasNeighbors;

    // Solo se resuelve (vía IsIrrigationDay) cuando Neighbors está vacío -- ver
    // IrrigationStatusViewModel.ResolveEmptyStateMessageAsync.
    public string EmptyStateMessage { get; set; } = string.Empty;
}

/// <summary>
/// Estado de riego por andador, visible para los 3 roles sin restricción: cualquier vecino debe
/// poder ver quién está regando y en qué andador. El backend no exige ningún permiso adicional
/// para GetOrganizationStatus -- solo estar autenticado en la organización.
/// </summary>
public partial class IrrigationStatusViewModel : ObservableObject
{
    private readonly IIrrigationService _irrigationService;
    private readonly IStructureService _structureService;
    private readonly IAlertService _alertService;
    private readonly ICurrentSession _currentSession;

    // Vocabulario exacto que devuelve el backend (NeighborIrrigationStatuses) -- Requested/Pending
    // ya vienen colapsados en "Waiting" antes de llegar aquí.
    internal const string WateringStatus = "Watering";
    internal const string WaitingStatus = "Waiting";
    internal const string CompletedStatus = "Completed";

    internal const string NoIrrigationScheduledMessage = "No hay riego programado hoy";
    internal const string NoActivityYetMessage = "Sin actividad todavía";

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<WalkwayStatusItem> Walkways { get; } = [];

    public IrrigationStatusViewModel(
        IIrrigationService irrigationService,
        IStructureService structureService,
        IAlertService alertService,
        ICurrentSession currentSession)
    {
        _irrigationService = irrigationService;
        _structureService = structureService;
        _alertService = alertService;
        _currentSession = currentSession;
    }

    [RelayCommand]
    internal async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var statusList = await _irrigationService.GetIrrigationStatusAsync();
            var myUserId = _currentSession.CachedUserId;

            Walkways.Clear();
            foreach (var walkway in statusList ?? [])
            {
                var item = new WalkwayStatusItem
                {
                    WalkwayId = walkway.WalkwayId,
                    WalkwayCode = walkway.WalkwayCode,
                    Neighbors = walkway.Neighbors.Select(n => new NeighborStatusItem
                    {
                        UserId = n.UserId,
                        TurnId = n.TurnId,
                        FullName = n.FullName,
                        RawStatus = n.Status,
                        StatusDisplay = TranslateStatus(n.Status),
                        IsMine = myUserId.HasValue && myUserId.Value == n.UserId
                    }).ToList()
                };

                if (!item.HasNeighbors)
                {
                    item.EmptyStateMessage = await ResolveEmptyStateMessageAsync(walkway.WalkwayId);
                }

                Walkways.Add(item);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Loading IrrigationStatus]: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task<string> ResolveEmptyStateMessageAsync(Guid walkwayId)
    {
        var walkwayDetail = await _structureService.GetWalkwayAsync(walkwayId);
        if (walkwayDetail is null)
        {
            // Fail-soft: sin saber el sector, no podemos afirmar que no hay riego programado.
            return NoActivityYetMessage;
        }

        var isIrrigationDay = await _irrigationService.IsIrrigationDayAsync(walkwayDetail.HydraulicSectorId);
        return isIrrigationDay ? NoActivityYetMessage : NoIrrigationScheduledMessage;
    }

    internal static string TranslateStatus(string rawStatus) => rawStatus switch
    {
        WateringStatus => "Regando",
        WaitingStatus => "Pendiente",
        CompletedStatus => "Terminado",
        _ => rawStatus
    };

    [RelayCommand]
    internal async Task StartTurnAsync(NeighborStatusItem? neighbor)
    {
        if (neighbor is null) return;

        var result = await _irrigationService.StartTurnAsync(neighbor.TurnId);
        await HandleActionResultAsync(result, AppStrings.TurnStartedSuccess);
    }

    [RelayCommand]
    internal async Task CompleteTurnAsync(NeighborStatusItem? neighbor)
    {
        if (neighbor is null) return;

        var result = await _irrigationService.CompleteTurnAsync(neighbor.TurnId);
        await HandleActionResultAsync(result, AppStrings.TurnCompletedSuccess);
    }

    private async Task HandleActionResultAsync(UserActionResult result, string successMessage)
    {
        if (result.IsSuccess)
        {
            await _alertService.ShowAsync(AppStrings.SuccessTitle, successMessage);
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
