using IrrigationMaster.Mobile.Application.Features.Models.Auth;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrrigationMaster.Mobile.Application.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(string email, string password);
        Task<UserActionResult> ChangePasswordAsync(string currentPassword, string newPassword, string confirmNewPassword);

        // Limpia cualquier cabecera Authorization que quedara pegada al HttpClient autenticado
        // compartido. Debe llamarse SIEMPRE junto con ICurrentSession.ClearAsync() al cerrar
        // sesión (ver CurrentSession.ClearAsync, que ya lo hace por delegación) -- de lo
        // contrario, la siguiente pantalla que reutilice ese HttpClient (incluida una pantalla
        // pensada para ser anónima) arrastraría el token de la sesión recién cerrada.
        void ClearAuthHeader();
    }
}
