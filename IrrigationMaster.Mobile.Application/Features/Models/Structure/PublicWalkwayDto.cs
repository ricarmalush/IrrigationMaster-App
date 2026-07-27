using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Structure;

// Espejo del PublicWalkwayDto del backend (Walkways/public, sin autenticación).
public class PublicWalkwayDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;
}
