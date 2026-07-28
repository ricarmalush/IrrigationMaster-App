using IrrigationMaster.Mobile.Application.Features.Models.Users;

namespace IrrigationMaster.Mobile.Application.Interfaces;

// Operaciones autenticadas de gestión de usuarios/roles (pantalla del Presidente/Coordinador).
// El backend es quien decide si el llamador tiene permiso (APPROVE_USERS/ASSIGN_WALKWAY/
// CHANGE_USER_ROLE) -- este servicio no duplica esa lógica, solo transporta la petición y
// devuelve tal cual lo que el backend responda.
public interface IUserManagementService
{
    Task<List<AppUserDto>?> GetUsersAsync(bool? isActive);
    Task<List<RoleDto>?> GetRolesAsync();
    Task<List<WalkwayDto>?> GetWalkwaysAsync();
    Task<UserActionResult> ActivateUserAsync(Guid userId);
    Task<UserActionResult> AssignWalkwayAsync(Guid userId, Guid? walkwayId);
    Task<UserActionResult> ChangeRoleAsync(Guid userId, Guid roleId);
}
