namespace IrrigationMaster.Mobile.Application.Features.Models.Users;

// Espejo de CreateUserCommand del backend (Users/Create, [AllowAnonymous]).
// El registro anónimo ya no envía OrganizationId: el backend resuelve la organización a partir
// de InvitationCode (el código que el vecino recibe de su Presidente/Coordinador).
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
