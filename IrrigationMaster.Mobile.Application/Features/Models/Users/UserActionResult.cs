using System.Text.Json.Serialization;
using IrrigationMaster.Mobile.Application.Common.Dtos;

namespace IrrigationMaster.Mobile.Application.Features.Models.Users;

// Espejo de Response&lt;bool&gt; del backend para Activate/AssignWalkway/ChangeRole. Sin campo Data:
// ninguna de estas acciones necesita el bool devuelto, solo IsSuccess/Message/Errors.
public class UserActionResult
{
    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("errors")]
    public List<ApiError>? Errors { get; init; }
}
