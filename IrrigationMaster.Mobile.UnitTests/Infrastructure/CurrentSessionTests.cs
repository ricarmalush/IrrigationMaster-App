using IrrigationMaster.Mobile.Infrastructure;
using IrrigationMaster.Mobile.UnitTests.TestDoubles;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IrrigationMaster.Mobile.UnitTests.Infrastructure;

public class CurrentSessionTests
{
    private static string BuildJwt(IEnumerable<Claim> claims)
        => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(claims: claims));

    [Fact]
    public async Task EstablishAsync_WithValidToken_SavesOrganizationIdAndRole()
    {
        var tokenStorage = new FakeTokenStorage();
        var session = new CurrentSession(tokenStorage);
        var organizationId = Guid.NewGuid().ToString();
        var jwt = BuildJwt([
            new Claim("organizationId", organizationId),
            new Claim(ClaimTypes.Role, "Admin")
        ]);

        await session.EstablishAsync(jwt);

        Assert.Equal(jwt, tokenStorage.SavedToken);
        Assert.Equal(organizationId, tokenStorage.SavedOrganizationId);
        Assert.Equal("Admin", tokenStorage.SavedRole);
    }

    [Fact]
    public async Task EstablishAsync_WithTokenMissingClaims_SavesEmptyValues()
    {
        var tokenStorage = new FakeTokenStorage();
        var session = new CurrentSession(tokenStorage);
        var jwt = BuildJwt([]);

        await session.EstablishAsync(jwt);

        Assert.Equal(string.Empty, tokenStorage.SavedOrganizationId);
        Assert.Equal(string.Empty, tokenStorage.SavedRole);
    }

    [Fact]
    public async Task EstablishAsync_WithMalformedToken_Throws()
    {
        var session = new CurrentSession(new FakeTokenStorage());

        await Assert.ThrowsAnyAsync<Exception>(() => session.EstablishAsync("esto-no-es-un-jwt"));
    }

    [Fact]
    public async Task GetOrganizationIdAsync_DelegatesToTokenStorage()
    {
        var tokenStorage = new FakeTokenStorage { StoredOrganizationId = "org-123" };
        var session = new CurrentSession(tokenStorage);

        var result = await session.GetOrganizationIdAsync();

        Assert.Equal("org-123", result);
    }

    [Fact]
    public async Task GetRoleAsync_DelegatesToTokenStorage()
    {
        var tokenStorage = new FakeTokenStorage { StoredRole = "SUPERADMIN" };
        var session = new CurrentSession(tokenStorage);

        var result = await session.GetRoleAsync();

        Assert.Equal("SUPERADMIN", result);
    }

    [Fact]
    public async Task ClearAsync_DelegatesToTokenStorage()
    {
        var tokenStorage = new FakeTokenStorage();
        var session = new CurrentSession(tokenStorage);

        await session.ClearAsync();

        Assert.True(tokenStorage.ClearCalled);
    }
}
