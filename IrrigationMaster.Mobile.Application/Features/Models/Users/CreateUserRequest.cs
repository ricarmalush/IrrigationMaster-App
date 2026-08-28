namespace IrrigationMaster.Mobile.Application.Features.Models.Users;

// Espejo de RegisterUserCommand del backend (Users/Register, [AllowAnonymous]) -- ruta separada de
// Users/Create (autenticada, exige el permiso CREATE_USERS) tras el diagnóstico de seguridad.
// El registro anónimo no envía OrganizationId: el backend resuelve la organización a partir de
// InvitationCode (el código que el vecino recibe de su Presidente/Coordinador). RoleId se sigue
// enviando por compatibilidad con TenantConfig.DefaultVecinoRoleId, pero el backend ya no lo lee
// en absoluto en esta ruta -- el rol se resuelve siempre a VECINO por Code, server-side.
public class CreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string InvitationCode { get; set; } = string.Empty;
    public Guid RoleId { get; set; }
    public string Password { get; set; } = string.Empty;
    public Guid? WalkwayId { get; set; }
}
