using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrrigationMaster.Mobile.Application.Interfaces;
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

// Fila de "Riego en tiempo real". IsCompleted alimenta el DataTrigger de color en el XAML (verde
// mientras riega, gris cuando ya terminó) -- sin exponer Color aquí, para que esta clase (y el
// ViewModel que la construye) se puedan seguir testeando sin runtime MAUI.
public class LiveTurnItem
{
    public Guid TurnId { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string RawStatus { get; init; } = string.Empty;
    public string StatusDisplay { get; init; } = string.Empty;

    public bool IsCompleted => RawStatus == IrrigationStatusViewModel.CompletedStatus;
}

/// <summary>
/// Visibilidad en tiempo real del riego del propio andador del llamador (pantalla "Mi Riego"):
/// solicitudes para mañana y riego de hoy. Complementa -- no sustituye -- "Estado de Riego"
/// (org-wide, todos los andadores). El backend no exige ningún permiso adicional ni gate de
/// licencia -- visible para cualquier autenticado de la organización, sin restricción de rol.
/// </summary>
public partial class MyIrrigationViewModel : ObservableObject
{
    private readonly IIrrigationService _irrigationService;

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

    public ObservableCollection<RequestedTurnItem> RequestsTomorrow { get; } = [];
    public ObservableCollection<LiveTurnItem> LiveToday { get; } = [];

    public MyIrrigationViewModel(IIrrigationService irrigationService)
    {
        _irrigationService = irrigationService;
    }

    [RelayCommand]
    internal async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var status = await _irrigationService.GetMyWalkwayStatusAsync();

            HasWalkway = status?.WalkwayId is not null;
            WalkwayCode = status?.WalkwayCode ?? string.Empty;

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

            LiveToday.Clear();
            foreach (var turn in status?.LiveToday ?? [])
            {
                LiveToday.Add(new LiveTurnItem
                {
                    TurnId = turn.TurnId,
                    FullName = turn.FullName,
                    RawStatus = turn.Status,
                    StatusDisplay = IrrigationStatusViewModel.TranslateStatus(turn.Status)
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
        }
    }
}
