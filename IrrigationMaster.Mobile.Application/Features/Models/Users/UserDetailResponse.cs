using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Users;

// Espejo de Response<UserResponseDto> del backend (Users/Get/{id}).
public class UserDetailResponse
{
    [JsonPropertyName("data")]
    public AppUserDto? Data { get; init; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;
}
