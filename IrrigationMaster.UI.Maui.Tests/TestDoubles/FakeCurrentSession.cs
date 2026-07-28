using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class FakeCurrentSession : ICurrentSession
{
    public string? EstablishedWithToken { get; private set; }
    public string? OrganizationIdToReturn { get; set; }
    public string? RoleToReturn { get; set; }

    public Task EstablishAsync(string jwtToken)
    {
        EstablishedWithToken = jwtToken;
        return Task.CompletedTask;
    }

    public Task<string?> GetOrganizationIdAsync() => Task.FromResult(OrganizationIdToReturn);
    public Task<string?> GetRoleAsync() => Task.FromResult(RoleToReturn);
    public Task ClearAsync() => Task.CompletedTask;
}
