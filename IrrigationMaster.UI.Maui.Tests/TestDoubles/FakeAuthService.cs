using IrrigationMaster.Mobile.Application.Features.Models.Auth;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class FakeAuthService : IAuthService
{
    public LoginResponse? ResponseToReturn { get; set; }
    public Exception? ExceptionToThrow { get; set; }
    public (string Email, string Password)? LastCall { get; private set; }

    public UserActionResult ChangePasswordResult { get; set; } = new() { IsSuccess = true };
    public (string CurrentPassword, string NewPassword, string ConfirmNewPassword)? LastChangePasswordCall { get; private set; }

    public bool ClearAuthHeaderCalled { get; private set; }

    public Task<LoginResponse?> LoginAsync(string email, string password)
    {
        LastCall = (email, password);

        if (ExceptionToThrow != null)
            throw ExceptionToThrow;

        return Task.FromResult(ResponseToReturn);
    }

    public Task<UserActionResult> ChangePasswordAsync(string currentPassword, string newPassword, string confirmNewPassword)
    {
        LastChangePasswordCall = (currentPassword, newPassword, confirmNewPassword);
        return Task.FromResult(ChangePasswordResult);
    }

    public void ClearAuthHeader() => ClearAuthHeaderCalled = true;
}
