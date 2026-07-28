using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Users;

// Espejo de UserResponseDto del backend (Users/pagination).
public class AppUserDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("firstName")]
    public string FirstName { get; init; } = string.Empty;

    [JsonPropertyName("lastName")]
    public string LastName { get; init; } = string.Empty;

    [JsonPropertyName("email")]
    public string Email { get; init; } = string.Empty;

    [JsonPropertyName("organizationId")]
    public Guid OrganizationId { get; init; }

    [JsonPropertyName("role")]
    public string Role { get; init; } = string.Empty;

    [JsonPropertyName("isActive")]
    public bool IsActive { get; init; }

    [JsonPropertyName("fullName")]
    public string FullName { get; init; } = string.Empty;

    [JsonPropertyName("walkwayId")]
    public Guid? WalkwayId { get; init; }

    [JsonPropertyName("walkwayCode")]
    public string? WalkwayCode { get; init; }
}
