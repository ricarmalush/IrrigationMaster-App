using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Common.Dtos;

// Espejo de IrrigationMaster.Transversal.Common.BaseError del backend.
public class ApiError
{
    [JsonPropertyName("propertyMessage")]
    public string PropertyMessage { get; init; } = string.Empty;

    [JsonPropertyName("errorMessage")]
    public string ErrorMessage { get; init; } = string.Empty;
}
