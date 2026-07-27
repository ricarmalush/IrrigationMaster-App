using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Core.Features.Models.Auth;

public class LoginResponse
{
    // Cambiado a 'init' para inmutabilidad y marcado como JsonPropertyName para asegurar el mapeo
    [JsonPropertyName("data")]
    public string? Data { get; init; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty; // 🟢 Evitamos nulos incómodos en la UI

    // 🟢 'Errors' tipado como un diccionario de strings para procesar errores de validación (como FluentValidation o ModelState)
    [JsonPropertyName("errors")]
    public System.Collections.Generic.Dictionary<string, string[]>? Errors { get; init; }
}