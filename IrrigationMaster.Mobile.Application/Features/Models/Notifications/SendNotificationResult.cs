using System.Text.Json.Serialization;
using IrrigationMaster.Mobile.Application.Common.Dtos;

namespace IrrigationMaster.Mobile.Application.Features.Models.Notifications;

// Espejo de Response<int> del backend para Notifications/Send: a diferencia de UserActionResult,
// aquí sí necesitamos el "Data" (el conteo real de destinatarios notificados) para construir el
// mensaje de confirmación ("Aviso enviado a X destinatarios").
public class SendNotificationResult
{
    [JsonPropertyName("data")]
    public int RecipientCount { get; init; }

    [JsonPropertyName("isSuccess")]
    public bool IsSuccess { get; init; }

    [JsonPropertyName("message")]
    public string Message { get; init; } = string.Empty;

    [JsonPropertyName("errors")]
    public List<ApiError>? Errors { get; init; }
}
