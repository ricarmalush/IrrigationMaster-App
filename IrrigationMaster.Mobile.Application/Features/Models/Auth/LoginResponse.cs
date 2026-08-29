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

    // No viene del JSON: ApiService lo rellena a partir del status code (402 Payment Required),
    // igual que isLicenceError en el Front Angular -- así el ViewModel distingue "sin licencia
    // activa" de un fallo de credenciales normal sin tener que comparar el texto del mensaje.
    [JsonIgnore]
    public bool IsLicenceError { get; init; }

    // No viene del JSON: ApiService lo rellena a partir del status code (403 Forbidden), que el
    // backend usa específicamente para "cuenta desactivada deliberadamente por un admin" -- ver
    // AuthController.Login. Distinto de IsLicenceError y del 401 genérico (credenciales
    // inválidas/pendiente de aprobación).
    [JsonIgnore]
    public bool IsAccountDeactivated { get; init; }
}