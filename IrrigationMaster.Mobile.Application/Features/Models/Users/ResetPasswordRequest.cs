namespace IrrigationMaster.Mobile.Application.Features.Models.Users;

// Espejo de ResetPasswordCommand del backend (Users/ResetPassword/{id}). El Id viaja en la URL;
// el backend lo inyecta en el comando (command with { Id = id }), así que el body solo necesita
// la contraseña nueva. Distinto de ChangePasswordRequest: no exige la contraseña actual porque
// quien resetea es un tercero de confianza (SUPERADMIN o Presidente con RESET_USER_PASSWORD),
// no el propio usuario.
public class ResetPasswordRequest
{
    public string NewPassword { get; set; } = string.Empty;
}
