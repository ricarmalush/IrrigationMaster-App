using System.Text.Json.Serialization;

namespace IrrigationMaster.Mobile.Application.Features.Models.Invoices;

// Espejo (parcial) de InvoiceResponseDto del backend (Invoices/MyInvoices) -- solo trae lo que
// necesita la pantalla "Mis Facturas": OrganizationId/OrderId/UserId/AssignedLicenseId no se usan
// aquí (esta pantalla ya viene filtrada por el propio usuario, vía el backend).
public class InvoiceDto
{
    [JsonPropertyName("id")]
    public Guid Id { get; init; }

    [JsonPropertyName("invoiceNumber")]
    public string InvoiceNumber { get; init; } = string.Empty;

    [JsonPropertyName("issueDate")]
    public DateTime IssueDate { get; init; }

    [JsonPropertyName("dueDate")]
    public DateTime DueDate { get; init; }

    [JsonPropertyName("totalAmountValue")]
    public decimal TotalAmountValue { get; init; }

    [JsonPropertyName("totalAmountCurrency")]
    public string TotalAmountCurrency { get; init; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; init; } = string.Empty;

    public bool IsPaid => string.Equals(Status, "Paid", StringComparison.OrdinalIgnoreCase);
}
