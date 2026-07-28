using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Users;

// Espejo (parcial) de WalkwayResponseDto del backend (Walkways/pagination, autenticado y
// acotado a la organización del llamador vía ICurrentUser). Distinto de PublicWalkwayDto: ese es
// para el flujo anónimo de auto-registro; este es para pantallas con sesión.
public class WalkwayDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;
}
