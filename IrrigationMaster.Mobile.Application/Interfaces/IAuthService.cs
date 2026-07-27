using IrrigationMaster.Mobile.Application.Features.Models.Auth;
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
    }
}
