namespace IrrigationMaster.Mobile.Application.Interfaces;

public interface ITokenStorage
{
    Task<string?> GetTokenAsync();
    Task<string?> GetOrganizationIdAsync();
    Task<string?> GetRoleAsync();
    Task SaveSessionAsync(string token, string organizationId, string role);
    Task ClearSessionAsync();
}