using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Structure;

// Espejo de Response<OrganizationResponseDto> del backend.
public class OrganizationDetailsResponse
{
    [JsonPropertyName("data")]
    public OrganizationDto? Data { get; init; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}
