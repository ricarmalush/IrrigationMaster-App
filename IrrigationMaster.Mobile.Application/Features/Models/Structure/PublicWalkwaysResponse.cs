using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Structure;

// Espejo de Response<IEnumerable<PublicWalkwayDto>> del backend.
public class PublicWalkwaysResponse
{
    [JsonPropertyName("data")]
    public List<PublicWalkwayDto>? Data { get; init; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}
