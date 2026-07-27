using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class FakeTokenStorage : ITokenStorage
{
    public string? StoredToken { get; set; }
    public string? StoredOrganizationId { get; set; }
    public string? StoredRole { get; set; }

    public Task<string?> GetTokenAsync() => Task.FromResult(StoredToken);
    public Task<string?> GetOrganizationIdAsync() => Task.FromResult(StoredOrganizationId);
    public Task<string?> GetRoleAsync() => Task.FromResult(StoredRole);

    public Task SaveSessionAsync(string token, string organizationId, string role)
    {
        StoredToken = token;
        StoredOrganizationId = organizationId;
        StoredRole = role;
        return Task.CompletedTask;
    }

    public Task ClearSessionAsync()
    {
        StoredToken = null;
        StoredOrganizationId = null;
        StoredRole = null;
        return Task.CompletedTask;
    }
}
