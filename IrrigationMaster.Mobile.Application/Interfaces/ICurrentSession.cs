namespace IrrigationMaster.Mobile.Application.Interfaces;

// Fuente única de verdad sobre "quién soy / de qué organización soy".
// Nada fuera de la implementación debe leer el JWT directamente.
public interface ICurrentSession
{
    Task EstablishAsync(string jwtToken);
    Task<string?> GetOrganizationIdAsync();
    Task<string?> GetRoleAsync();
    Task ClearAsync();
}
