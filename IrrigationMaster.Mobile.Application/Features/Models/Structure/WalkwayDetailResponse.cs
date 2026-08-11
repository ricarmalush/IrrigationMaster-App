using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Structure;

// Espejo de Response<WalkwayResponseDto> del backend (Walkways/Get/{id}).
public class WalkwayDetailResponse
{
    [JsonPropertyName("data")]
    public WalkwayDetailDto? Data { get; init; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}
