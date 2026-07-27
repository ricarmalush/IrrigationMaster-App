using IrrigationMaster.Mobile.Core.Features.Models.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrrigationMaster.Mobile.Core.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponse?> LoginAsync(string email, string password);
    }
}
