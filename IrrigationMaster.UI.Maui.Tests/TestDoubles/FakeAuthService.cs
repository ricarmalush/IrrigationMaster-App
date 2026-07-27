using IrrigationMaster.Mobile.Application.Features.Models.Auth;
using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class FakeAuthService : IAuthService
{
    public LoginResponse? ResponseToReturn { get; set; }
    public Exception? ExceptionToThrow { get; set; }
    public (string Email, string Password)? LastCall { get; private set; }

    public Task<LoginResponse?> LoginAsync(string email, string password)
    {
        LastCall = (email, password);

        if (ExceptionToThrow != null)
            throw ExceptionToThrow;

        return Task.FromResult(ResponseToReturn);
    }
}
