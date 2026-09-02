namespace IrrigationMaster.Mobile.Application.Features.Models.Users;

// Espejo de UpdateUserCommand del backend (PUT Users/Update/{id}). No incluye RoleId/WalkwayId --
// esos tienen su propio flujo dedicado (ChangeRoleAsync/AssignWalkwayAsync). Cualquier autenticado
// puede usarlo sobre su PROPIO Id sin ningún permiso especial (auto-edición, ver
// UpdateUserCommandHandler); para editar a otro usuario de la organización hace falta el permiso
// VIEW_ORG_USERS -- esta capa no duplica esa comprobación, solo transporta la petición.
public class UpdateUserRequest
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public string? Street { get; set; }
    public int? HouseNumber { get; set; }
}
