using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Structure.Country;

public class CountryDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    [JsonPropertyName("code")]
    public string Code { get; init; } = string.Empty;

    [JsonPropertyName("phoneCode")]
    public string PhoneCode { get; init; } = string.Empty;
}