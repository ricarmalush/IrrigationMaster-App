using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.UI.Maui.Common;
using System.Collections.ObjectModel;

namespace IrrigationMaster.UI.Maui.Features.Level3_Functional.Users;

// Clases auxiliares para los Picker de rol/andador (mismo patrón que CountryItem/HydraulicSectorItem
// en SystemSettingsViewModel).
public class RoleItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

public class WalkwayItem
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
}

// Id null representa "Todas las organizaciones" (sin filtro), siempre la primera entrada del
// desplegable. Solo lo usa SUPERADMIN -- ver ShowOrganizationFilter.
public class OrganizationFilterItem
{
    public Guid? Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Una fila de la lista. ObservableObject porque cada fila necesita su propio estado mutable
// (el rol/andador que el Presidente está eligiendo en el Picker de ESA fila, antes de confirmar).
public partial class UserListItem : ObservableObject
{
    public Guid Id { get; init; }
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Role { get; init; } = string.Empty;
    public bool IsActive { get; init; }
    public string WalkwayDisplay { get; init; } = "Sin asignar";
    public string OrganizationName { get; init; } = string.Empty;

    public string StatusDisplay => IsActive ? "Activo" : "Pendiente";
    public bool ShowApprove => !IsActive;

    // Picker en Windows tiene un bug conocido y sin arreglo oficial (dotnet/maui #5038, #24369):
    // no refresca su texto visible al cambiar SelectedItem, ni al precargarlo por binding ni tras
    // una selección real del usuario. En vez de depender de ese texto, exponemos aquí el valor
    // seleccionado ya formateado para un Label aparte que sí se actualiza de forma fiable.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RoleSelectionDisplay))]
    public partial RoleItem? SelectedRole { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(WalkwaySelectionDisplay))]
    public partial WalkwayItem? SelectedWalkway { get; set; }

    public string RoleSelectionDisplay => SelectedRole?.Name ?? "Selecciona un rol";
    public string WalkwaySelectionDisplay => SelectedWalkway?.Code ?? "Sin asignar";

    // Restablecer contraseña: solo dos campos, sin la contraseña actual -- quien la resetea es un
    // tercero de confianza (Presidente/SUPERADMIN), no el propio usuario. Estado por fila, igual
    // que SelectedRole/SelectedWalkway, para no interferir entre tarjetas distintas.
    [ObservableProperty] public partial string NewPassword { get; set; } = string.Empty;
    [ObservableProperty] public partial string ConfirmNewPassword { get; set; } = string.Empty;
}

/// <summary>
/// Gestión de usuarios/roles para el Presidente/Coordinador: aprobar pendientes, asignar andador,
/// cambiar rol. La autorización real la decide el backend (APPROVE_USERS/ASSIGN_WALKWAY/
/// CHANGE_USER_ROLE); esta pantalla solo muestra el error tal cual si el backend rechaza la acción.
/// </summary>
public partial class UserManagementViewModel : ObservableObject
{
    private readonly IUserManagementService _userManagementService;
    private readonly IStructureService _structureService;
    private readonly IAlertService _alertService;

    private const string SuperAdminRoleCode = "SUPERADMIN";

    // Evita el doble LoadAsync reentrante: poblar OrganizationFilters fija
    // SelectedOrganizationFilter a "Todas", lo que dispara OnSelectedOrganizationFilterChanged;
    // esta bandera hace que esa primera asignación (interna, no del usuario) no dispare una
    // segunda carga por encima de la que ya está en curso.
    private bool _isPopulatingOrganizationFilters;

    [ObservableProperty] public partial bool ShowOnlyPending { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy => !IsBusy;

    // Visible solo para SUPERADMIN: el resto de roles ya viene acotado a su propia organización
    // por el backend (filtro multi-tenant global), no necesitan elegir cuál ver.
    [ObservableProperty] public partial bool ShowOrganizationFilter { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(OrganizationFilterSelectionDisplay))]
    public partial OrganizationFilterItem? SelectedOrganizationFilter { get; set; }

    // Mismo motivo que RoleSelectionDisplay/WalkwaySelectionDisplay: Picker en Windows no
    // refresca su texto visible al cambiar SelectedItem (bug conocido, dotnet/maui #5038/#24369).
    public string OrganizationFilterSelectionDisplay => SelectedOrganizationFilter?.Name ?? "Todas las organizaciones";

    public ObservableCollection<UserListItem> Users { get; } = [];
    public ObservableCollection<RoleItem> Roles { get; } = [];
    public ObservableCollection<WalkwayItem> Walkways { get; } = [];
    public ObservableCollection<OrganizationFilterItem> OrganizationFilters { get; } = [];

    public UserManagementViewModel(
        IUserManagementService userManagementService,
        IStructureService structureService,
        IAlertService alertService,
        ICurrentSession currentSession)
    {
        _userManagementService = userManagementService;
        _structureService = structureService;
        _alertService = alertService;

        ShowOrganizationFilter = string.Equals(currentSession.CachedRole, SuperAdminRoleCode, StringComparison.OrdinalIgnoreCase);
    }

    partial void OnSelectedOrganizationFilterChanged(OrganizationFilterItem? value)
    {
        if (_isPopulatingOrganizationFilters) return;
        _ = LoadAsync();
    }

    [RelayCommand]
    internal async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            // Roles/Walkways están acotados automáticamente a la organización del llamador vía
            // ICurrentUser en el backend. Users también lo está para cualquier rol que no sea
            // SUPERADMIN; para SUPERADMIN se puede acotar manualmente a una organización concreta
            // eligiéndola en el desplegable (SelectedOrganizationFilter), o dejar sin filtro
            // ("Todas las organizaciones") para verlas todas mezcladas.
            if (ShowOrganizationFilter && OrganizationFilters.Count == 0)
            {
                await PopulateOrganizationFiltersAsync();
            }

            var rolesTask = _userManagementService.GetRolesAsync();
            var walkwaysTask = _userManagementService.GetWalkwaysAsync();
            var usersTask = _userManagementService.GetUsersAsync(ShowOnlyPending ? false : null, SelectedOrganizationFilter?.Id);

            await Task.WhenAll(rolesTask, walkwaysTask, usersTask);

            Roles.Clear();
            // Solo se descarta el rol de plataforma SUPERADMIN por Code: el backend lo rechazaría
            // igualmente, pero listarlo aquí confundiría al Presidente. NO se filtra por
            // OrganizationId == Guid.Empty -- otros roles de plantilla globales por diseño
            // (VECINO, PRESIDENTE...) también lo tienen y sí deben poder asignarse.
            const string superAdminRoleCode = "SUPERADMIN";
            foreach (var role in (rolesTask.Result ?? []).Where(r => r.Code != superAdminRoleCode))
            {
                Roles.Add(new RoleItem { Id = role.Id, Name = role.Name });
            }

            Walkways.Clear();
            foreach (var walkway in walkwaysTask.Result ?? [])
            {
                Walkways.Add(new WalkwayItem { Id = walkway.Id, Code = walkway.Code });
            }

            Users.Clear();
            foreach (var user in usersTask.Result ?? [])
            {
                var item = new UserListItem
                {
                    Id = user.Id,
                    FullName = $"{user.FirstName} {user.LastName}",
                    Email = user.Email,
                    Role = user.Role,
                    IsActive = user.IsActive,
                    WalkwayDisplay = user.WalkwayCode ?? "Sin asignar",
                    OrganizationName = user.OrganizationName,
                    SelectedRole = Roles.FirstOrDefault(r => r.Name == user.Role),
                    SelectedWalkway = user.WalkwayId.HasValue ? Walkways.FirstOrDefault(w => w.Id == user.WalkwayId.Value) : null
                };
                Users.Add(item);
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Loading UserManagement]: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task PopulateOrganizationFiltersAsync()
    {
        _isPopulatingOrganizationFilters = true;
        try
        {
            OrganizationFilters.Clear();
            OrganizationFilters.Add(new OrganizationFilterItem { Id = null, Name = "Todas las organizaciones" });

            var organizations = await _structureService.GetOrganizationsAsync();
            foreach (var org in organizations ?? [])
            {
                OrganizationFilters.Add(new OrganizationFilterItem { Id = org.Id, Name = org.Name });
            }

            SelectedOrganizationFilter = OrganizationFilters[0];
        }
        finally
        {
            _isPopulatingOrganizationFilters = false;
        }
    }

    [RelayCommand]
    internal async Task ShowPendingAsync()
    {
        ShowOnlyPending = true;
        await LoadAsync();
    }

    [RelayCommand]
    internal async Task ShowAllAsync()
    {
        ShowOnlyPending = false;
        await LoadAsync();
    }

    [RelayCommand]
    internal async Task ApproveAsync(UserListItem? user)
    {
        if (user is null) return;

        var result = await _userManagementService.ActivateUserAsync(user.Id);
        await HandleActionResultAsync(result, AppStrings.UserApprovedSuccess);
    }

    [RelayCommand]
    internal async Task AssignWalkwayAsync(UserListItem? user)
    {
        if (user is null) return;

        var result = await _userManagementService.AssignWalkwayAsync(user.Id, user.SelectedWalkway?.Id);
        await HandleActionResultAsync(result, AppStrings.WalkwayAssignedSuccess);
    }

    [RelayCommand]
    internal async Task ChangeRoleAsync(UserListItem? user)
    {
        if (user is null) return;

        if (user.SelectedRole is null)
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgSelectRoleFirst);
            return;
        }

        var result = await _userManagementService.ChangeRoleAsync(user.Id, user.SelectedRole.Id);
        await HandleActionResultAsync(result, AppStrings.RoleChangedSuccess);
    }

    [RelayCommand]
    internal async Task ResetPasswordAsync(UserListItem? user)
    {
        if (user is null) return;

        if (string.IsNullOrWhiteSpace(user.NewPassword) || string.IsNullOrWhiteSpace(user.ConfirmNewPassword))
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgMissingResetPasswordData);
            return;
        }

        // Validación local, sin red: mismo motivo que ExecuteChangePasswordAsync en
        // SystemSettingsViewModel -- el backend también la hace (y la mostraríamos igual, tal
        // cual, si llegara desde ahí), pero comprobarlo aquí evita una petición innecesaria y le
        // da al Presidente/SUPERADMIN el mismo mensaje al instante.
        if (user.NewPassword != user.ConfirmNewPassword)
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgPasswordsDoNotMatch);
            return;
        }

        var result = await _userManagementService.ResetPasswordAsync(user.Id, user.NewPassword);
        await HandleActionResultAsync(result, AppStrings.PasswordResetSuccess);
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
