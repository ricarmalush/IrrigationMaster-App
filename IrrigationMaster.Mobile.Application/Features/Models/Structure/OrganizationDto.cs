using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Structure;

// Espejo (parcial) de OrganizationResponseDto del backend (Organizations/Get/{id}). Solo trae lo
// que necesita la sección "Mi Organización" de Ajustes: nombre e InvitationCode para compartir.
public class OrganizationDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("invitationCode")]
    public string InvitationCode { get; init; } = string.Empty;
}
