namespace IrrigationMaster.Mobile.Application.Interfaces;

// Fuente única de verdad sobre "quién soy / de qué organización soy".
// Nada fuera de la implementación debe leer el JWT directamente.
public interface ICurrentSession
{
    Task EstablishAsync(string jwtToken);
    Task<string?> GetOrganizationIdAsync();
    Task<string?> GetRoleAsync();
    Task ClearAsync();

    // Copia en memoria del rol, poblada de forma síncrona por EstablishAsync (login) y borrada
    // por ClearAsync. A diferencia de GetRoleAsync (que siempre lee de SecureStorage), esta
    // propiedad permite decidir UI dependiente del rol de forma síncrona -- necesario para
    // construir un TabbedPage con las pestañas correctas ANTES de su primer render, en vez de
    // mutar sus Children después (lo que revienta en Android). Null antes de haber iniciado
    // sesión en este proceso.
    string? CachedRole { get; }
}
