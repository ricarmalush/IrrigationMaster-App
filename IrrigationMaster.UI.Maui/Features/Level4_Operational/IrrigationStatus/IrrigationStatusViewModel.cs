using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrrigationMaster.Mobile.Application.Features.Models.Irrigation;
using IrrigationMaster.Mobile.Application.Features.Models.Structure;
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

    // false mientras el turno sigue en Requested -- Start() lo rechaza hasta que alguien con
    // TURN_APPROVE/SUPERADMIN lo apruebe (ver ApproveTurnsViewModel).
    public bool IsApproved { get; init; }

    public bool ShowStartButton => IsMine && RawStatus == IrrigationStatusViewModel.WaitingStatus && IsApproved;
    public bool ShowCompleteButton => IsMine && RawStatus == IrrigationStatusViewModel.WateringStatus;
    public bool ShowWaitingApprovalLabel => IsMine && RawStatus == IrrigationStatusViewModel.WaitingStatus && !IsApproved;
}

public class WalkwayStatusItem
{
    public Guid WalkwayId { get; init; }
    public string WalkwayCode { get; init; } = string.Empty;
    public List<NeighborStatusItem> Neighbors { get; init; } = [];

    // Sector hidráulico de este andador, resuelto vía IStructureService.GetWalkwayAsync -- null si
    // no se pudo resolver. RequestTurnCommand lo necesita para poder solicitar un turno.
    public Guid? HydraulicSectorId { get; set; }

    // true solo en el andador del propio usuario logueado, cuando no tiene ningún turno hoy
    // todavía -- "Empezar mi turno" no tiene sentido sin un IrrigationTurn previo (ver diagnóstico
    // del flujo real), así que se ofrece "Solicitar mi turno" en su lugar.
    public bool CanRequestTurn { get; set; }

    public bool HasNeighbors => Neighbors.Count > 0;
    public bool ShowEmptyState => !HasNeighbors;

    // Solo se resuelve (vía IsIrrigationDay) cuando Neighbors está vacío -- ver
    // IrrigationStatusViewModel.ResolveEmptyStateMessageAsync.
    public string EmptyStateMessage { get; set; } = string.Empty;

    // Una línea por cada IrrigationProgram activo del sector de este andador (puede haber más de
    // uno, p. ej. uno para riego matutino y otro para nocturno). Vacío si no hay ninguno activo,
    // o si no se pudo resolver el sector del andador -- no confundir con EmptyStateMessage, que
    // habla del día concreto, no del patrón general del sector.
    public List<string> IrrigationPatternLines { get; set; } = [];
    public bool ShowIrrigationPattern => IrrigationPatternLines.Count > 0;
    public string IrrigationPatternText => string.Join("\n", IrrigationPatternLines);
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
    private readonly IUserManagementService _userManagementService;
    private readonly IAlertService _alertService;
    private readonly ICurrentSession _currentSession;

    // Vocabulario exacto que devuelve el backend (NeighborIrrigationStatuses) -- Requested/Pending
    // ya vienen colapsados en "Waiting" antes de llegar aquí.
    internal const string WateringStatus = "Watering";
    internal const string WaitingStatus = "Waiting";
    internal const string CompletedStatus = "Completed";

    internal const string NoIrrigationScheduledMessage = "No hay riego programado hoy";
    internal const string NoActivityYetMessage = "Sin actividad todavía";

    // Duración ad-hoc de un turno "Solicitar mi turno": no hay ningún horario preestablecido que
    // elegir (a diferencia de un IrrigationProgram), así que se pide un bloque fijo a partir de
    // ahora -- el Presidente/Vicepresidente que aprueba ve las horas exactas antes de aceptar.
    internal const int DefaultTurnDurationHours = 2;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<WalkwayStatusItem> Walkways { get; } = [];

    public IrrigationStatusViewModel(
        IIrrigationService irrigationService,
        IStructureService structureService,
        IUserManagementService userManagementService,
        IAlertService alertService,
        ICurrentSession currentSession)
    {
        _irrigationService = irrigationService;
        _structureService = structureService;
        _userManagementService = userManagementService;
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
            var programs = await _irrigationService.GetIrrigationProgramsAsync() ?? [];
            var myUserId = _currentSession.CachedUserId;

            // Necesario para saber en QUÉ andador ofrecer "Solicitar mi turno": el propio vecino
            // no aparece en ningún WalkwayIrrigationStatusDto.Neighbors si no tiene turno hoy, así
            // que no hay forma de deducir su andador a partir de statusList.
            var myWalkwayId = myUserId.HasValue
                ? (await _userManagementService.GetUserByIdAsync(myUserId.Value))?.WalkwayId
                : null;

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
                        IsMine = myUserId.HasValue && myUserId.Value == n.UserId,
                        IsApproved = n.IsApproved
                    }).ToList()
                };

                // Se resuelve el andador -> sector para TODOS los andadores (no solo los vacíos):
                // el patrón de riego es información del sector, independiente de si hoy hay
                // actividad o no. RequestTurnCommand también necesita este sector.
                var walkwayDetail = await _structureService.GetWalkwayAsync(walkway.WalkwayId);
                item.HydraulicSectorId = walkwayDetail?.HydraulicSectorId;

                item.CanRequestTurn = myWalkwayId.HasValue
                    && walkway.WalkwayId == myWalkwayId.Value
                    && item.HydraulicSectorId.HasValue
                    && !item.Neighbors.Any(n => n.IsMine);

                if (walkwayDetail is not null)
                {
                    item.IrrigationPatternLines = programs
                        .Where(p => p.IsActive && p.HydraulicSectorId == walkwayDetail.HydraulicSectorId)
                        .Select(p => BuildIrrigationPatternText(p))
                        .Where(text => text is not null)
                        .Select(text => text!)
                        .ToList();
                }

                if (!item.HasNeighbors)
                {
                    item.EmptyStateMessage = await ResolveEmptyStateMessageAsync(walkwayDetail);
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

    private async Task<string> ResolveEmptyStateMessageAsync(WalkwayDetailDto? walkwayDetail)
    {
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

    private static readonly string[] DayNames =
        ["Lunes", "Martes", "Miércoles", "Jueves", "Viernes", "Sábado", "Domingo"];

    private static readonly string[] MonthNames =
    [
        "enero", "febrero", "marzo", "abril", "mayo", "junio",
        "julio", "agosto", "septiembre", "octubre", "noviembre", "diciembre"
    ];

    // "Este sector riega: Sábado y Domingo, de marzo a noviembre" -- null si DaysOfWeek no trae
    // ningún día válido (defensivo: el backend no valida su formato, ver comentario en
    // IrrigationProgramDto).
    internal static string? BuildIrrigationPatternText(IrrigationProgramDto program)
    {
        var days = ParseDaysOfWeek(program.DaysOfWeek);
        if (days.Count == 0) return null;

        var text = $"Este sector riega: {JoinWithY(days)}";

        if (program.SeasonStartMonth.HasValue && program.SeasonEndMonth.HasValue
            && program.SeasonStartDay.HasValue && program.SeasonEndDay.HasValue)
        {
            text += $", de {MonthNames[program.SeasonStartMonth.Value - 1]} a {MonthNames[program.SeasonEndMonth.Value - 1]}";
        }

        return text;
    }

    // CSV de enteros ISO-8601 (Lunes=1...Domingo=7, p. ej. "1,3,5"); entradas no numéricas o fuera
    // de [1,7] se descartan en vez de reventar -- el backend no las valida en absoluto.
    private static List<string> ParseDaysOfWeek(string daysOfWeekCsv) => daysOfWeekCsv
        .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
        .Select(s => int.TryParse(s, out var day) ? day : (int?)null)
        .Where(day => day is >= 1 and <= 7)
        .Select(day => day!.Value)
        .Distinct()
        .OrderBy(day => day)
        .Select(day => DayNames[day - 1])
        .ToList();

    private static string JoinWithY(List<string> items) => items.Count switch
    {
        0 => string.Empty,
        1 => items[0],
        _ => string.Join(", ", items.Take(items.Count - 1)) + " y " + items[^1]
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

    [RelayCommand]
    internal async Task RequestTurnAsync(WalkwayStatusItem? walkway)
    {
        var myUserId = _currentSession.CachedUserId;
        if (walkway?.HydraulicSectorId is null || !myUserId.HasValue) return;

        var start = DateTime.UtcNow.AddMinutes(1);
        var end = start.AddHours(DefaultTurnDurationHours);

        var result = await _irrigationService.RequestTurnAsync(walkway.HydraulicSectorId.Value, myUserId.Value, start, end);
        await HandleActionResultAsync(result, AppStrings.TurnRequestedSuccess);
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
