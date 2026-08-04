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

    [ObservableProperty] public partial RoleItem? SelectedRole { get; set; }
    [ObservableProperty] public partial WalkwayItem? SelectedWalkway { get; set; }
}

/// <summary>
/// Gestión de usuarios/roles para el Presidente/Coordinador: aprobar pendientes, asignar andador,
/// cambiar rol. La autorización real la decide el backend (APPROVE_USERS/ASSIGN_WALKWAY/
/// CHANGE_USER_ROLE); esta pantalla solo muestra el error tal cual si el backend rechaza la acción.
/// </summary>
public partial class UserManagementViewModel : ObservableObject
{
    private readonly IUserManagementService _userManagementService;
    private readonly IAlertService _alertService;

    [ObservableProperty] public partial bool ShowOnlyPending { get; set; } = true;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<UserListItem> Users { get; } = [];
    public ObservableCollection<RoleItem> Roles { get; } = [];
    public ObservableCollection<WalkwayItem> Walkways { get; } = [];

    public UserManagementViewModel(
        IUserManagementService userManagementService,
        IAlertService alertService)
    {
        _userManagementService = userManagementService;
        _alertService = alertService;
    }

    [RelayCommand]
    internal async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            // Users/Roles/Walkways están todos acotados automáticamente a la organización del
            // llamador vía ICurrentUser en el backend -- no hace falta resolver ni pasar el
            // OrganizationId a mano desde aquí.
            var rolesTask = _userManagementService.GetRolesAsync();
            var walkwaysTask = _userManagementService.GetWalkwaysAsync();
            var usersTask = _userManagementService.GetUsersAsync(ShowOnlyPending ? false : null);

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
