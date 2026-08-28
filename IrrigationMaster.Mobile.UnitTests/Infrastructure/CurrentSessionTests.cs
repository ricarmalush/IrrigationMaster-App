using IrrigationMaster.Mobile.Infrastructure;
using IrrigationMaster.Mobile.UnitTests.TestDoubles;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IrrigationMaster.Mobile.UnitTests.Infrastructure;

public class CurrentSessionTests
{
    private static string BuildJwt(IEnumerable<Claim> claims)
        => new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(claims: claims));

    private static (CurrentSession Session, FakeTokenStorage TokenStorage, FakeAuthService AuthService) CreateSut()
    {
        var tokenStorage = new FakeTokenStorage();
        var authService = new FakeAuthService();
        var session = new CurrentSession(tokenStorage, authService);
        return (session, tokenStorage, authService);
    }

    [Fact]
    public async Task EstablishAsync_WithValidToken_SavesOrganizationIdAndRole()
    {
        var (session, tokenStorage, _) = CreateSut();
        var organizationId = Guid.NewGuid().ToString();
        var jwt = BuildJwt([
            new Claim("organizationId", organizationId),
            new Claim(ClaimTypes.Role, "Admin")
        ]);

        await session.EstablishAsync(jwt);

        Assert.Equal(jwt, tokenStorage.SavedToken);
        Assert.Equal(organizationId, tokenStorage.SavedOrganizationId);
        Assert.Equal("Admin", tokenStorage.SavedRole);
        // CachedRole debe quedar disponible de forma síncrona tras EstablishAsync -- es lo que
        // permite decidir UI dependiente del rol (p. ej. qué pestañas mostrar) sin esperar a
        // una lectura async de SecureStorage.
        Assert.Equal("Admin", session.CachedRole);
    }

    [Fact]
    public async Task EstablishAsync_WithTokenMissingClaims_SavesEmptyValues()
    {
        var (session, tokenStorage, _) = CreateSut();
        var jwt = BuildJwt([]);

        await session.EstablishAsync(jwt);

        Assert.Equal(string.Empty, tokenStorage.SavedOrganizationId);
        Assert.Equal(string.Empty, tokenStorage.SavedRole);
    }

    [Fact]
    public async Task EstablishAsync_WithMalformedToken_Throws()
    {
        var (session, _, _) = CreateSut();

        await Assert.ThrowsAnyAsync<Exception>(() => session.EstablishAsync("esto-no-es-un-jwt"));
    }

    [Fact]
    public async Task GetOrganizationIdAsync_DelegatesToTokenStorage()
    {
        var (session, tokenStorage, _) = CreateSut();
        tokenStorage.StoredOrganizationId = "org-123";

        var result = await session.GetOrganizationIdAsync();

        Assert.Equal("org-123", result);
    }

    [Fact]
    public async Task GetRoleAsync_DelegatesToTokenStorage()
    {
        var (session, tokenStorage, _) = CreateSut();
        tokenStorage.StoredRole = "SUPERADMIN";

        var result = await session.GetRoleAsync();

        Assert.Equal("SUPERADMIN", result);
    }

    [Fact]
    public async Task ClearAsync_DelegatesToTokenStorage()
    {
        var (session, tokenStorage, _) = CreateSut();

        await session.ClearAsync();

        Assert.True(tokenStorage.ClearCalled);
    }

    [Fact]
    public async Task ClearAsync_ClearsCachedRole()
    {
        var (session, _, _) = CreateSut();
        await session.EstablishAsync(BuildJwt([new Claim(ClaimTypes.Role, "SUPERADMIN")]));

        await session.ClearAsync();

        Assert.Null(session.CachedRole);
    }

    // ─── MECANISMO DE LOGOUT: limpieza de la cabecera Authorization del HttpClient compartido ───
    // No existe hoy ningún botón "Cerrar sesión" en la App, pero el mecanismo debe existir y
    // quedar testeado, para que cuando se añada no vuelva a fallar por lo mismo que RegisterAsync
    // (un HttpClient Singleton que arrastra el token de otra sesión entre pantallas).

    [Fact]
    public async Task ClearAsync_AlsoClearsTheSharedHttpClientAuthHeader()
    {
        var (session, _, authService) = CreateSut();

        await session.ClearAsync();

        Assert.True(authService.ClearAuthHeaderCalled);
    }

    [Fact]
    public async Task ClearAsync_ClearsAuthHeader_EvenWhenNoSessionWasEverEstablished()
    {
        // Defensivo: limpiar debe ser seguro de llamar en cualquier momento, no solo tras un login
        // real -- un futuro botón "Cerrar sesión" podría dispararse en estados inesperados.
        var (session, _, authService) = CreateSut();

        await session.ClearAsync();

        Assert.True(authService.ClearAuthHeaderCalled);
    }

    [Fact]
    public void CachedRole_BeforeEstablishAsync_IsNull()
    {
        var (session, _, _) = CreateSut();

        Assert.Null(session.CachedRole);
    }
}
