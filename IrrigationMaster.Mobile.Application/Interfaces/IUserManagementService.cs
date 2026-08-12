using IrrigationMaster.Mobile.Application.Features.Models.Users;

namespace IrrigationMaster.Mobile.Application.Interfaces;

// Operaciones autenticadas de gestión de usuarios/roles (pantalla del Presidente/Coordinador).
// El backend es quien decide si el llamador tiene permiso (APPROVE_USERS/ASSIGN_WALKWAY/
// CHANGE_USER_ROLE/RESET_USER_PASSWORD) -- este servicio no duplica esa lógica, solo transporta
// la petición y devuelve tal cual lo que el backend responda.
public interface IUserManagementService
{
    // organizationId solo tiene efecto para SUPERADMIN (el backend lo ignora para cualquier otro
    // rol, que ya viene acotado a su propia organización): null trae todas.
    Task<List<AppUserDto>?> GetUsersAsync(bool? isActive, Guid? organizationId = null);

    // Detalle de un único usuario -- usado para resolver el andador propio del llamador (p. ej.
    // "Avisar a mi comunidad" necesita saber si el Presidente/SUPERADMIN logueado tiene un andador
    // asignado, dato que no viaja en el JWT).
    Task<AppUserDto?> GetUserByIdAsync(Guid userId);
    Task<List<RoleDto>?> GetRolesAsync();
    Task<List<WalkwayDto>?> GetWalkwaysAsync();
    Task<UserActionResult> ActivateUserAsync(Guid userId);
    Task<UserActionResult> AssignWalkwayAsync(Guid userId, Guid? walkwayId);
    Task<UserActionResult> ChangeRoleAsync(Guid userId, Guid roleId);

    // Distinto de IAuthService.ChangePasswordAsync: aquí un tercero de confianza (SUPERADMIN o
    // Presidente con RESET_USER_PASSWORD) establece la contraseña de OTRO usuario sin conocer la
    // anterior -- pensado para el vecino que olvidó la suya.
    Task<UserActionResult> ResetPasswordAsync(Guid userId, string newPassword);
}
