using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrrigationMaster.Mobile.Core.Features.Models.Auth
{
    public enum AuthResultCode
    {
        Success = 1,
        InvalidCredentials = 2,
        UserNotFound = 3,
        AccountLocked = 4
    }
}
