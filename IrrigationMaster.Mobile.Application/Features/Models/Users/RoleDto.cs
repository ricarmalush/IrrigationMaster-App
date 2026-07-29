using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Users;

// Espejo (parcial) de RoleResponseDto del backend (Roles/pagination). Code permite descartar del
// Picker específicamente el rol de plataforma SUPERADMIN: el backend ya lo rechazaría igualmente,
// pero listarlo confundiría al Presidente. OrganizationId == Guid.Empty NO sirve para este filtro,
// porque otros roles de plantilla globales por diseño (VECINO, PRESIDENTE...) también lo tienen y
// sí deben poder asignarse.
public class RoleDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;

    [JsonPropertyName("organizationId")]
    public Guid OrganizationId { get; init; }
}
