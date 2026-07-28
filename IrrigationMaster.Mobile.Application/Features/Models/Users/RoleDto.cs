using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Users;

// Espejo (parcial) de RoleResponseDto del backend (Roles/pagination). OrganizationId permite
// descartar del Picker los roles globales del sistema (Guid.Empty, p. ej. SUPERADMIN): el backend
// ya lo rechazaría igualmente, pero listarlo confundiría al Presidente.
public class RoleDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("organizationId")]
    public Guid OrganizationId { get; init; }
}
