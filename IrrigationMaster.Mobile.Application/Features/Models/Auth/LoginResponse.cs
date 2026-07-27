using IrrigationMaster.Mobile.Application.Common.Dtos;
using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Auth;

public class LoginResponse
{
    [JsonPropertyName("data")]
    public string? Data { get; init; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    // El backend serializa IEnumerable<BaseError> como array de objetos, no como diccionario.
    [JsonPropertyName("errors")]
    public List<ApiError>? Errors { get; init; }
}