using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Common.Dtos;

// Espejo de Response<string> del backend para RegenerateInvitationCode -- a diferencia de
// StructureOperationResult (Data: Guid?, usado en las operaciones de creación), aquí Data es el
// nuevo código de invitación generado.
public class RegenerateInvitationCodeResult
{
    [JsonPropertyName("data")]
    public string? Data { get; init; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("errors")]
    public List<ApiError>? Errors { get; init; }
}
