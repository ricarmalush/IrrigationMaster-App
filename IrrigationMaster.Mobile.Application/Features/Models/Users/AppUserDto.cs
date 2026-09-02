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

    [JsonPropertyName("organizationName")]
    public string OrganizationName { get; init; } = string.Empty;

    // El backend ya lo manda en cada fila de Users/pagination (UserProfile.cs, ConstructUsing) --
    // hasta ahora la App lo descartaba en silencio al no tener una propiedad que lo recogiera.
    // Distingue "nunca aprobado" (IsActive=false, DeactivatedAt=null) de "desactivado
    // deliberadamente por un admin" (IsActive=false, DeactivatedAt con valor) -- ambos mostraban
    // el mismo "Pendiente" en UserManagementPage. DeactivatedBy no se expone todavía: no hace
    // falta para esta distinción, solo para mostrar "quién" (decisión explícita: no por ahora).
    [JsonPropertyName("deactivatedAt")]
    public DateTime? DeactivatedAt { get; init; }

    // Dirección: ambos opcionales, no mostrados todavía en ninguna pantalla de la App (no existe
    // una "ficha de usuario" de creación/edición aquí, a diferencia de Angular -- UserManagementPage
    // es de solo lectura + acciones puntuales). Se reciben ya para no perder el dato si en el
    // futuro se añade esa pantalla.
    [JsonPropertyName("street")]
    public string? Street { get; init; }

    [JsonPropertyName("houseNumber")]
    public int? HouseNumber { get; init; }
}
