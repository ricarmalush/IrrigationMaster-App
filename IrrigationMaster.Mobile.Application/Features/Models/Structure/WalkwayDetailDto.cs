using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Structure;

// Espejo (parcial) de WalkwayResponseDto del backend (Walkways/Get/{id}). Distinto de
// IrrigationMaster.Mobile.Application.Features.Models.Users.WalkwayDto (que solo trae Id+Code
// para los Picker de asignación): este trae también HydraulicSectorId, necesario para consultar
// IrrigationPrograms/IsIrrigationDay sobre un andador sin actividad hoy.
public class WalkwayDetailDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;

    [JsonPropertyName("hydraulicSectorId")]
    public Guid HydraulicSectorId { get; init; }
}
