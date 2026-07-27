// UI.Maui/Services/SecureTokenStorage.cs
using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Services;

public class SecureTokenStorage : ITokenStorage
{
    private const string TokenKey = "jwt_token";
    private const string OrgKey = "organization_id";
    private const string RoleKey = "user_role";

    public Task<string?> GetTokenAsync() => SecureStorage.Default.GetAsync(TokenKey);
    public Task<string?> GetOrganizationIdAsync() => SecureStorage.Default.GetAsync(OrgKey);
    public Task<string?> GetRoleAsync() => SecureStorage.Default.GetAsync(RoleKey);

    public async Task SaveSessionAsync(string token, string organizationId, string role)
    {
        await SecureStorage.Default.SetAsync(TokenKey, token);
        await SecureStorage.Default.SetAsync(OrgKey, organizationId);
        await SecureStorage.Default.SetAsync(RoleKey, role);
    }

    public Task ClearSessionAsync()
    {
        SecureStorage.Default.Remove(TokenKey);
        SecureStorage.Default.Remove(OrgKey);
        SecureStorage.Default.Remove(RoleKey);
        return Task.CompletedTask;
    }
}