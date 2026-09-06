using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.IrrigationStatus;
using System.Collections.ObjectModel;

namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.MyIrrigation;

// Fila de "Solicitudes para mañana". Plana (no ObservableObject): se reconstruye entera en cada
// LoadAsync, igual que NeighborStatusItem/WalkwayStatusItem en la vista hermana "Estado de Riego".
public class RequestedTurnItem
{
    public Guid TurnId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public DateTime ScheduledStart { get; init; }

    public string ScheduledStartDisplay => ScheduledStart.ToString("HH:mm");
}

// Fila de "Riego en tiempo real" -- propia y de otros vecinos del mismo andador en
// Requested/InProgress/Completed de hoy. IsCompleted alimenta el DataTrigger de color en el XAML
// (verde mientras riega, gris cuando ya terminó) -- sin exponer Color aquí, para que esta clase (y
// el ViewModel que la construye) se puedan seguir testeando sin runtime MAUI.
public class LiveTurnItem
{
    public Guid UserId { get; init; }
    public Guid TurnId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string RawStatus { get; init; } = string.Empty;
    public string StatusDisplay { get; init; } = string.Empty;

    // "¿Es esta la fila del usuario logueado?" -- espejo de NeighborStatusItem.IsMine en la vista
    // hermana "Estado de Riego". El backend ya bloquea a nivel de negocio que alguien actúe sobre
    // el turno de otro vecino; esta comparación es solo conveniencia de UI.
    public bool IsMine { get; init; }

    public bool IsCompleted => RawStatus == IrrigationStatusViewModel.CompletedStatus;

    // Ya no exige aprobación previa -- confirmado con el Presidente, ese paso desaparece del ciclo
    // por completo: Requested (Waiting) es accionable de inmediato (Empezar o Cancelar).
    public bool ShowStartButton => IsMine && RawStatus == IrrigationStatusViewModel.WaitingStatus;
    public bool ShowCancelButton => IsMine && RawStatus == IrrigationStatusViewModel.WaitingStatus;
    public bool ShowCompleteButton => IsMine && RawStatus == IrrigationStatusViewModel.WateringStatus;
}

/// <summary>
/// Visibilidad en tiempo real del riego del propio andador del llamador (pantalla "Mi Riego"):
/// solicitudes para mañana y riego de hoy. Complementa -- no sustituye -- "Estado de Riego"
/// (org-wide, todos los andadores). El backend no exige ningún permiso adicional ni gate de
/// licencia -- visible para cualquier autenticado de la organización, sin restricción de rol.
///
/// "Solicitar mi turno" vive aquí también (antes solo en la vista hermana): para Vecino, esta
/// pantalla ES su "Estado de Riego" desde que AdminMenuPage la redirige aquí, y es la ÚNICA a la
/// que tiene acceso. Empezar/Cancelar/Terminar turno viven AQUÍ TAMBIÉN desde que el ciclo dejó de
/// tener aprobación: antes se asumía que la vista hermana ya cubría estas acciones "para todos los
/// roles con acceso a ella", pero un Vecino nunca tuvo acceso a esa vista -- sin esto, no tenía
/// ninguna forma de empezar ni terminar su propio turno. LiveToday incluye ahora también los
/// turnos Requested de hoy (propios y de otros vecinos del mismo andador), ordenados por
/// HouseNumber descendente (puramente informativo, nunca bloqueante -- ver
/// NeighborIrrigationStatusDto en el backend).
/// </summary>
public partial class MyIrrigationViewModel : ObservableObject
{
    private readonly IIrrigationService _irrigationService;
    private readonly IStructureService _structureService;
    private readonly ICurrentSession _currentSession;
    private readonly IAlertService _alertService;

    // Mismo bloque fijo de 2h desde "ahora + 1 min" que RequestTurnAsync en IrrigationStatusViewModel.
    internal const int DefaultTurnDurationHours = 2;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy => !IsBusy;

    // Un caller sin andador asignado (p. ej. un Presidente) recibe WalkwayId=null del backend --
    // estado válido, no un error: se muestra un mensaje simple en vez de las dos listas.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(NoWalkwayAssigned))]
    public partial bool HasWalkway { get; set; }

    public bool NoWalkwayAssigned => !HasWalkway;

    [ObservableProperty] public partial string WalkwayCode { get; set; } = string.Empty;

    // Resuelto tras cargar el estado (WalkwayId -> HydraulicSectorId, ver LoadAsync) -- lo exige
    // RequestTurnAsync, igual que HydraulicSectorId en WalkwayStatusItem de la vista hermana.
    private Guid? _hydraulicSectorId;

    public ObservableCollection<RequestedTurnItem> RequestsTomorrow { get; } = [];
    public ObservableCollection<LiveTurnItem> LiveToday { get; } = [];

    // Espejo de CanRequestTurn en WalkwayStatusItem (vista hermana): solo si hoy no tienes ya
    // ningún turno (en cualquier estado) en tu andador. LiveToday ya está acotado siempre al propio
    // andador del llamador (server-side), así que no hace falta comparar WalkwayId aquí. Propiedad
    // derivada normal (no [ObservableProperty]: depende de varios campos que cambian juntos en
    // LoadAsync) -- notificada a mano al final de LoadAsync.
    public bool CanRequestTurn =>
        _hydraulicSectorId.HasValue
        && _currentSession.CachedUserId.HasValue
        && !LiveToday.Any(t => t.UserId == _currentSession.CachedUserId.Value);

    public MyIrrigationViewModel(IIrrigationService irrigationService, IStructureService structureService, ICurrentSession currentSession, IAlertService alertService)
    {
        _irrigationService = irrigationService;
        _structureService = structureService;
        _currentSession = currentSession;
        _alertService = alertService;
    }

    [RelayCommand]
    internal async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var status = await _irrigationService.GetMyWalkwayStatusAsync();
            var myUserId = _currentSession.CachedUserId;

            HasWalkway = status?.WalkwayId is not null;
            WalkwayCode = status?.WalkwayCode ?? string.Empty;

            _hydraulicSectorId = status?.WalkwayId.HasValue == true
                ? (await _structureService.GetWalkwayAsync(status.WalkwayId.Value))?.HydraulicSectorId
                : null;

            // Ya vienen ordenadas por ScheduledStart desde el backend -- no hace falta reordenar.
            RequestsTomorrow.Clear();
            foreach (var turn in status?.RequestsTomorrow ?? [])
            {
                RequestsTomorrow.Add(new RequestedTurnItem
                {
                    TurnId = turn.TurnId,
                    FullName = turn.FullName,
                    ScheduledStart = turn.ScheduledStart
                });
            }

            // Ya viene ordenada por HouseNumber descendente desde el backend (informativo) -- no
            // hace falta reordenar.
            LiveToday.Clear();
            foreach (var turn in status?.LiveToday ?? [])
            {
                LiveToday.Add(new LiveTurnItem
                {
                    UserId = turn.UserId,
                    TurnId = turn.TurnId,
                    FullName = turn.FullName,
                    RawStatus = turn.Status,
                    StatusDisplay = IrrigationStatusViewModel.TranslateStatus(turn.Status),
                    IsMine = myUserId.HasValue && myUserId.Value == turn.UserId
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Loading MyIrrigation]: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
            OnPropertyChanged(nameof(CanRequestTurn));
        }
    }

    [RelayCommand]
    internal async Task RequestTurnAsync()
    {
        var myUserId = _currentSession.CachedUserId;
        if (!CanRequestTurn || _hydraulicSectorId is null || !myUserId.HasValue) return;

        var start = DateTime.UtcNow.AddMinutes(1);
        var end = start.AddHours(DefaultTurnDurationHours);

        var result = await _irrigationService.RequestTurnAsync(_hydraulicSectorId.Value, myUserId.Value, start, end);
        await HandleActionResultAsync(result, AppStrings.TurnRequestedSuccess);
    }

    [RelayCommand]
    internal async Task StartTurnAsync(LiveTurnItem? turn)
    {
        if (turn is null || !turn.ShowStartButton) return;

        var result = await _irrigationService.StartTurnAsync(turn.TurnId);
        await HandleActionResultAsync(result, AppStrings.TurnStartedSuccess);
    }

    [RelayCommand]
    internal async Task CancelTurnAsync(LiveTurnItem? turn)
    {
        if (turn is null || !turn.ShowCancelButton) return;

        var result = await _irrigationService.CancelTurnAsync(turn.TurnId);
        await HandleActionResultAsync(result, AppStrings.TurnCancelledSuccess);
    }

    [RelayCommand]
    internal async Task CompleteTurnAsync(LiveTurnItem? turn)
    {
        if (turn is null || !turn.ShowCompleteButton) return;

        var result = await _irrigationService.CompleteTurnAsync(turn.TurnId);
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
