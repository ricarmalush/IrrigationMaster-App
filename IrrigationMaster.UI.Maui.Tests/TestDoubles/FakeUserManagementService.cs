using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class FakeUserManagementService : IUserManagementService
{
    public List<AppUserDto>? UsersToReturn { get; set; } = [];
    public AppUserDto? UserByIdToReturn { get; set; }
    public List<RoleDto>? RolesToReturn { get; set; } = [];
    public List<WalkwayDto>? WalkwaysToReturn { get; set; } = [];
    public UserActionResult ActivateResult { get; set; } = new() { IsSuccess = true };
    public UserActionResult AssignWalkwayResult { get; set; } = new() { IsSuccess = true };
    public UserActionResult ChangeRoleResult { get; set; } = new() { IsSuccess = true };
    public UserActionResult ResetPasswordResult { get; set; } = new() { IsSuccess = true };

    public Guid? LastGetUserByIdCall { get; private set; }
    public bool? LastIsActiveFilter { get; private set; }
    public Guid? LastOrganizationIdFilter { get; private set; }
    public Guid? LastActivatedUserId { get; private set; }
    public (Guid UserId, Guid? WalkwayId)? LastAssignWalkwayCall { get; private set; }
    public (Guid UserId, Guid RoleId)? LastChangeRoleCall { get; private set; }
    public (Guid UserId, string NewPassword)? LastResetPasswordCall { get; private set; }

    public Task<List<AppUserDto>?> GetUsersAsync(bool? isActive, Guid? organizationId = null)
    {
        LastIsActiveFilter = isActive;
        LastOrganizationIdFilter = organizationId;
        return Task.FromResult(UsersToReturn);
    }

    public Task<AppUserDto?> GetUserByIdAsync(Guid userId)
    {
        LastGetUserByIdCall = userId;
        return Task.FromResult(UserByIdToReturn);
    }

    public Task<List<RoleDto>?> GetRolesAsync() => Task.FromResult(RolesToReturn);

    public Task<List<WalkwayDto>?> GetWalkwaysAsync() => Task.FromResult(WalkwaysToReturn);

    public Task<UserActionResult> ActivateUserAsync(Guid userId)
    {
        LastActivatedUserId = userId;
        return Task.FromResult(ActivateResult);
    }

    public Task<UserActionResult> AssignWalkwayAsync(Guid userId, Guid? walkwayId)
    {
        LastAssignWalkwayCall = (userId, walkwayId);
        return Task.FromResult(AssignWalkwayResult);
    }

    public Task<UserActionResult> ChangeRoleAsync(Guid userId, Guid roleId)
    {
        LastChangeRoleCall = (userId, roleId);
        return Task.FromResult(ChangeRoleResult);
    }

    public Task<UserActionResult> ResetPasswordAsync(Guid userId, string newPassword)
    {
        LastResetPasswordCall = (userId, newPassword);
        return Task.FromResult(ResetPasswordResult);
    }
}
