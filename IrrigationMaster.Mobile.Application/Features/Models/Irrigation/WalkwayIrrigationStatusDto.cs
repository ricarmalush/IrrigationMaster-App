using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Irrigation;

// Espejo de WalkwayIrrigationStatusDto del backend. Neighbors viene vacío cuando el andador no
// tiene ningún turno para la fecha consultada -- no hace falta una segunda llamada a Walkways
// para saber qué andadores existen, el backend ya los incluye todos siempre.
public class WalkwayIrrigationStatusDto
{
    [JsonPropertyName("walkwayId")]
    public Guid WalkwayId { get; init; }

    [JsonPropertyName("walkwayCode")]
    public string WalkwayCode { get; init; } = string.Empty;

    [JsonPropertyName("neighbors")]
    public List<NeighborIrrigationStatusDto> Neighbors { get; init; } = [];
}
