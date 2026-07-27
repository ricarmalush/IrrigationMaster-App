namespace IrrigationMaster.Mobile.Application.Features.Models.Users;

// Espejo de CreateUserCommand del backend (Users/Create, [AllowAnonymous]).
public class CreateUserRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public Guid OrganizationId { get; set; }
    public Guid RoleId { get; set; }
    public string Password { get; set; } = string.Empty;
    public Guid? WalkwayId { get; set; }
}
