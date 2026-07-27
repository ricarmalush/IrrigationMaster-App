using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Common.Dtos;

// Espejo de Response<Guid> del backend para las operaciones de creación (Organization/HydraulicSector/Walkway).
public class StructureOperationResult
{
    [JsonPropertyName("data")]
    public Guid? Data { get; init; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("errors")]
    public List<ApiError>? Errors { get; init; }
}
