using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrrigationMaster.Mobile.Application.Features.Models.Irrigation;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.UI.Maui.Common;
using System.Collections.ObjectModel;

namespace IrrigationMaster.UI.Maui.Features.Level3_Functional.IrrigationPrograms;

// Mismo patrón que CountryItem/HydraulicSectorItem en SystemSettingsViewModel: clase auxiliar
// pequeña por ViewModel para alimentar un Picker, no compartida entre pantallas a propósito.
public class HydraulicSectorItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Una fila del listado. Plantilla plana (no ObservableObject): se reconstruye entera en cada
// LoadAsync, igual que NeighborStatusItem/NotificationItem -- no hay estado mutable propio.
public class IrrigationProgramListItem
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public TimeSpan StartTime { get; init; }
    public int DurationMinutes { get; init; }
    public string DaysOfWeek { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public Guid HydraulicSectorId { get; init; }
    public string HydraulicSectorName { get; init; } = string.Empty;
    public int? SeasonStartMonth { get; init; }
    public int? SeasonStartDay { get; init; }
    public int? SeasonEndMonth { get; init; }
    public int? SeasonEndDay { get; init; }

    public string StartTimeDisplay => StartTime.ToString(@"hh\:mm");
    public string StatusDisplay => IsActive ? "Activo" : "Inactivo";
}

/// <summary>
/// Calendario de riego: crear y editar IrrigationProgram. Visible en AdminMenuPage para
/// SUPERADMIN/Coordinador de Riego (ver AdminMenuPage.ComputeMenuVisibility) -- el backend exige
/// el mismo criterio (SUPERADMIN o el permiso MANAGE_IRRIGATION_PROGRAMS), esta pantalla no
/// duplica esa comprobación, solo transporta la petición.
///
/// Formulario compartido entre crear y editar (mismo patrón que UserListItem.SelectedRole/
/// SelectedWalkway, pero a nivel de página en vez de por fila): EditingProgramId null significa
/// "creando uno nuevo"; con valor, "editando ese programa". El sector hidráulico NO es editable
/// (el backend no lo admite en Update -- ver UpdateIrrigationProgramRequest), por eso
/// CanChangeHydraulicSector se apaga en modo edición.
/// </summary>
public partial class IrrigationProgramsViewModel : ObservableObject
{
    private readonly IIrrigationService _irrigationService;
    private readonly IStructureService _structureService;
    private readonly IAlertService _alertService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<IrrigationProgramListItem> Programs { get; } = [];
    public ObservableCollection<HydraulicSectorItem> HydraulicSectors { get; } = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEditing))]
    [NotifyPropertyChangedFor(nameof(FormTitle))]
    [NotifyPropertyChangedFor(nameof(SaveButtonText))]
    [NotifyPropertyChangedFor(nameof(CanChangeHydraulicSector))]
    public partial Guid? EditingProgramId { get; set; }

    [ObservableProperty] public partial string Name { get; set; } = string.Empty;
    [ObservableProperty] public partial TimeSpan StartTime { get; set; } = new(6, 0, 0);
    [ObservableProperty] public partial string DurationMinutesText { get; set; } = string.Empty;
    [ObservableProperty] public partial string DaysOfWeekText { get; set; } = string.Empty;
    [ObservableProperty] public partial HydraulicSectorItem? SelectedHydraulicSector { get; set; }
    [ObservableProperty] public partial bool IsActive { get; set; } = true;
    [ObservableProperty] public partial string SeasonStartMonthText { get; set; } = string.Empty;
    [ObservableProperty] public partial string SeasonStartDayText { get; set; } = string.Empty;
    [ObservableProperty] public partial string SeasonEndMonthText { get; set; } = string.Empty;
    [ObservableProperty] public partial string SeasonEndDayText { get; set; } = string.Empty;

    public bool IsEditing => EditingProgramId.HasValue;
    public string FormTitle => IsEditing ? "Editar programa de riego" : "Nuevo programa de riego";
    public string SaveButtonText => IsEditing ? "Guardar cambios" : "Crear programa";
    public bool CanChangeHydraulicSector => !IsEditing;

    public IrrigationProgramsViewModel(IIrrigationService irrigationService, IStructureService structureService, IAlertService alertService)
    {
        _irrigationService = irrigationService;
        _structureService = structureService;
        _alertService = alertService;
    }

    [RelayCommand]
    internal async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var sectorsTask = _structureService.GetHydraulicSectorsAsync();
            var programsTask = _irrigationService.GetIrrigationProgramsAsync();
            await Task.WhenAll(sectorsTask, programsTask);

            HydraulicSectors.Clear();
            foreach (var sector in sectorsTask.Result ?? [])
            {
                HydraulicSectors.Add(new HydraulicSectorItem { Id = sector.Id, Name = sector.Name });
            }

            Programs.Clear();
            foreach (var program in programsTask.Result ?? [])
            {
                Programs.Add(new IrrigationProgramListItem
                {
                    Id = program.Id,
                    Name = program.Name,
                    StartTime = program.StartTime,
                    DurationMinutes = program.DurationMinutes,
                    DaysOfWeek = program.DaysOfWeek,
                    IsActive = program.IsActive,
                    HydraulicSectorId = program.HydraulicSectorId,
                    HydraulicSectorName = HydraulicSectors.FirstOrDefault(s => s.Id == program.HydraulicSectorId)?.Name ?? "Sector desconocido",
                    SeasonStartMonth = program.SeasonStartMonth,
                    SeasonStartDay = program.SeasonStartDay,
                    SeasonEndMonth = program.SeasonEndMonth,
                    SeasonEndDay = program.SeasonEndDay
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Loading IrrigationPrograms]: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    internal void EditProgram(IrrigationProgramListItem? program)
    {
        if (program is null) return;

        EditingProgramId = program.Id;
        Name = program.Name;
        StartTime = program.StartTime;
        DurationMinutesText = program.DurationMinutes.ToString();
        DaysOfWeekText = program.DaysOfWeek;
        SelectedHydraulicSector = HydraulicSectors.FirstOrDefault(s => s.Id == program.HydraulicSectorId);
        IsActive = program.IsActive;
        SeasonStartMonthText = program.SeasonStartMonth?.ToString() ?? string.Empty;
        SeasonStartDayText = program.SeasonStartDay?.ToString() ?? string.Empty;
        SeasonEndMonthText = program.SeasonEndMonth?.ToString() ?? string.Empty;
        SeasonEndDayText = program.SeasonEndDay?.ToString() ?? string.Empty;
    }

    [RelayCommand]
    internal void CancelEdit() => ResetForm();

    private void ResetForm()
    {
        EditingProgramId = null;
        Name = string.Empty;
        StartTime = new TimeSpan(6, 0, 0);
        DurationMinutesText = string.Empty;
        DaysOfWeekText = string.Empty;
        SelectedHydraulicSector = null;
        IsActive = true;
        SeasonStartMonthText = string.Empty;
        SeasonStartDayText = string.Empty;
        SeasonEndMonthText = string.Empty;
        SeasonEndDayText = string.Empty;
    }

    [RelayCommand]
    internal async Task SaveAsync()
    {
        if (string.IsNullOrWhiteSpace(Name) || string.IsNullOrWhiteSpace(DaysOfWeekText) ||
            !int.TryParse(DurationMinutesText, out var durationMinutes))
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgMissingIrrigationProgramData);
            return;
        }

        if (!IsEditing && SelectedHydraulicSector is null)
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgMissingIrrigationProgramData);
            return;
        }

        if (!TryParseSeason(SeasonStartMonthText, SeasonStartDayText, SeasonEndMonthText, SeasonEndDayText,
                out var seasonStartMonth, out var seasonStartDay, out var seasonEndMonth, out var seasonEndDay))
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgInvalidSeasonData);
            return;
        }

        IsBusy = true;
        try
        {
            var result = IsEditing
                ? await _irrigationService.UpdateIrrigationProgramAsync(EditingProgramId!.Value, new UpdateIrrigationProgramRequest
                {
                    Name = Name.Trim(),
                    StartTime = StartTime,
                    DurationMinutes = durationMinutes,
                    DaysOfWeek = DaysOfWeekText.Trim(),
                    IsActive = IsActive,
                    SeasonStartMonth = seasonStartMonth,
                    SeasonStartDay = seasonStartDay,
                    SeasonEndMonth = seasonEndMonth,
                    SeasonEndDay = seasonEndDay
                })
                : await _irrigationService.CreateIrrigationProgramAsync(new CreateIrrigationProgramRequest
                {
                    Name = Name.Trim(),
                    StartTime = StartTime,
                    DurationMinutes = durationMinutes,
                    DaysOfWeek = DaysOfWeekText.Trim(),
                    HydraulicSectorId = SelectedHydraulicSector!.Id,
                    SeasonStartMonth = seasonStartMonth,
                    SeasonStartDay = seasonStartDay,
                    SeasonEndMonth = seasonEndMonth,
                    SeasonEndDay = seasonEndDay
                });

            if (result.IsSuccess)
            {
                await _alertService.ShowAsync(AppStrings.SuccessTitle,
                    IsEditing ? AppStrings.IrrigationProgramUpdatedSuccess : AppStrings.IrrigationProgramCreatedSuccess);
                ResetForm();
                await LoadAsync();
            }
            else
            {
                await _alertService.ShowAsync(AppStrings.ErrorTitle, BuildFailureMessage(result));
            }
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Mismo criterio "todo o nada" que valida el backend (IrrigationProgramValidatorExtensions.
    // ApplySeasonRules): los 4 campos vacíos es válido (sin temporada); una mezcla de vacíos y
    // rellenos, o un valor no numérico, no lo es. El rango real de mes/día lo sigue validando el
    // backend -- aquí solo se evita una llamada de red con datos a medio rellenar.
    internal static bool TryParseSeason(
        string startMonthText, string startDayText, string endMonthText, string endDayText,
        out int? startMonth, out int? startDay, out int? endMonth, out int? endDay)
    {
        startMonth = startDay = endMonth = endDay = null;

        var allEmpty = string.IsNullOrWhiteSpace(startMonthText) && string.IsNullOrWhiteSpace(startDayText) &&
                        string.IsNullOrWhiteSpace(endMonthText) && string.IsNullOrWhiteSpace(endDayText);
        if (allEmpty) return true;

        var allFilled = !string.IsNullOrWhiteSpace(startMonthText) && !string.IsNullOrWhiteSpace(startDayText) &&
                         !string.IsNullOrWhiteSpace(endMonthText) && !string.IsNullOrWhiteSpace(endDayText);
        if (!allFilled) return false;

        if (!int.TryParse(startMonthText, out var sm) || !int.TryParse(startDayText, out var sd) ||
            !int.TryParse(endMonthText, out var em) || !int.TryParse(endDayText, out var ed))
            return false;

        startMonth = sm;
        startDay = sd;
        endMonth = em;
        endDay = ed;
        return true;
    }

    private static string BuildFailureMessage(UserActionResult result)
    {
        if (result.Errors is { Count: > 0 })
            return string.Join("\n", result.Errors.Select(e => $"{e.PropertyMessage}: {e.ErrorMessage}"));

        return string.IsNullOrWhiteSpace(result.Message) ? AppStrings.ApiConnectionError : result.Message;
    }
}
