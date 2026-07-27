using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class FakeCurrentSession : ICurrentSession
{
    public string? EstablishedWithToken { get; private set; }

    public Task EstablishAsync(string jwtToken)
    {
        EstablishedWithToken = jwtToken;
        return Task.CompletedTask;
    }

    public Task<string?> GetOrganizationIdAsync() => Task.FromResult<string?>(null);
    public Task<string?> GetRoleAsync() => Task.FromResult<string?>(null);
    public Task ClearAsync() => Task.CompletedTask;
}
