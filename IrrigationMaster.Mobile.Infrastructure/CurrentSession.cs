using IrrigationMaster.Mobile.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IrrigationMaster.Mobile.Infrastructure;

public class CurrentSession : ICurrentSession
{
    private readonly ITokenStorage _tokenStorage;

    public CurrentSession(ITokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    public string? CachedRole { get; private set; }

    public async Task EstablishAsync(string jwtToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var parsedToken = handler.ReadJwtToken(jwtToken);

        var organizationId = parsedToken.Claims.FirstOrDefault(c => c.Type == "organizationId")?.Value ?? string.Empty;
        var role = parsedToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;

        await _tokenStorage.SaveSessionAsync(jwtToken, organizationId, role);
        CachedRole = role;
    }

    public Task<string?> GetOrganizationIdAsync() => _tokenStorage.GetOrganizationIdAsync();

    public Task<string?> GetRoleAsync() => _tokenStorage.GetRoleAsync();

    public Task ClearAsync()
    {
        CachedRole = null;
        return _tokenStorage.ClearSessionAsync();
    }
}
