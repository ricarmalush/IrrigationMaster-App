namespace IrrigationMaster.Mobile.Application.Features.Models.Users;

// Espejo de ChangeRoleCommand del backend (Users/ChangeRole/{id}). El Id viaja en la URL;
// el backend lo inyecta en el comando (command with { Id = id }), así que el body solo
// necesita el nuevo rol.
public class ChangeRoleRequest
{
    public Guid RoleId { get; set; }
}
