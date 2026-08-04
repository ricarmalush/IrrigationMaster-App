namespace IrrigationMaster.Mobile.Application.Features.Models.Users;

// Espejo de ChangePasswordCommand del backend (Users/ChangePassword). El usuario objetivo
// se resuelve en el backend a partir del JWT (ICurrentUser), no viaja en el body.
public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
