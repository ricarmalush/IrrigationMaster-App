using IrrigationMaster.Mobile.Application.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace IrrigationMaster.Mobile.Infrastructure;

public class CurrentSession : ICurrentSession
{
    private readonly ITokenStorage _tokenStorage;
    private readonly IAuthService _authService;

    public CurrentSession(ITokenStorage tokenStorage, IAuthService authService)
    {
        _tokenStorage = tokenStorage;
        _authService = authService;
    }

    public string? CachedRole { get; private set; }
    public Guid? CachedUserId { get; private set; }

    public async Task EstablishAsync(string jwtToken)
    {
        var handler = new JwtSecurityTokenHandler();
        var parsedToken = handler.ReadJwtToken(jwtToken);

        var organizationId = parsedToken.Claims.FirstOrDefault(c => c.Type == "organizationId")?.Value ?? string.Empty;
        var role = parsedToken.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value ?? string.Empty;
        var userIdClaim = parsedToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Sub)?.Value;

        await _tokenStorage.SaveSessionAsync(jwtToken, organizationId, role);
        CachedRole = role;
        CachedUserId = Guid.TryParse(userIdClaim, out var userId) ? userId : null;
    }

    public Task<string?> GetOrganizationIdAsync() => _tokenStorage.GetOrganizationIdAsync();

    public Task<string?> GetRoleAsync() => _tokenStorage.GetRoleAsync();

    // No existe hoy ningún botón "Cerrar sesión" en la App (ver diagnóstico de seguridad), pero
    // este método ya deja el mecanismo completo listo para cuando se añada: limpia el
    // almacenamiento de sesión Y la cabecera Authorization del HttpClient autenticado compartido.
    // Si algún día se llama solo a _tokenStorage.ClearSessionAsync() sin pasar por aquí, la
    // siguiente pantalla que reutilice ese HttpClient (incluida una pensada para ser anónima)
    // volvería a arrastrar el token de la sesión recién cerrada.
    public Task ClearAsync()
    {
        CachedRole = null;
        CachedUserId = null;
        _authService.ClearAuthHeader();
        return _tokenStorage.ClearSessionAsync();
    }
}
