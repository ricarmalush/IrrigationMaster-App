using IrrigationMaster.Mobile.Application.Features.Models.Auth;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.Mobile.UnitTests.TestDoubles;

public class FakeAuthService : IAuthService
{
    public LoginResponse? ResponseToReturn { get; set; }
    public UserActionResult ChangePasswordResult { get; set; } = new() { IsSuccess = true };
    public bool ClearAuthHeaderCalled { get; private set; }

    public Task<LoginResponse?> LoginAsync(string email, string password) => Task.FromResult(ResponseToReturn);

    public Task<UserActionResult> ChangePasswordAsync(string currentPassword, string newPassword, string confirmNewPassword)
        => Task.FromResult(ChangePasswordResult);

    public void ClearAuthHeader() => ClearAuthHeaderCalled = true;
}
