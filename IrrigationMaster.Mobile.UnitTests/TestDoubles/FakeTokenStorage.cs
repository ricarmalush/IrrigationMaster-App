using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.Mobile.UnitTests.TestDoubles;

public class FakeTokenStorage : ITokenStorage
{
    public string? StoredToken { get; set; }
    public string? StoredOrganizationId { get; set; }
    public string? StoredRole { get; set; }

    public string? SavedToken { get; private set; }
    public string? SavedOrganizationId { get; private set; }
    public string? SavedRole { get; private set; }
    public bool ClearCalled { get; private set; }

    public Task<string?> GetTokenAsync() => Task.FromResult(StoredToken);
    public Task<string?> GetOrganizationIdAsync() => Task.FromResult(StoredOrganizationId);
    public Task<string?> GetRoleAsync() => Task.FromResult(StoredRole);

    public Task SaveSessionAsync(string token, string organizationId, string role)
    {
        SavedToken = token;
        SavedOrganizationId = organizationId;
        SavedRole = role;
        return Task.CompletedTask;
    }

    public Task ClearSessionAsync()
    {
        ClearCalled = true;
        return Task.CompletedTask;
    }
}
