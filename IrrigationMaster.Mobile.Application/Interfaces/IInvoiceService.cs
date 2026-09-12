using IrrigationMaster.Mobile.Application.Features.Models.Invoices;

namespace IrrigationMaster.Mobile.Application.Interfaces;

// Autoservicio de licencia INDIVIDUAL del propio usuario (Invoices/MyInvoices) -- sin permiso
// especial, el backend filtra por Invoice.UserId == el propio caller. Nada que ver con
// Invoices/Mine (organización completa, gestión), que esta App no expone.
public interface IInvoiceService
{
    Task<List<InvoiceDto>?> GetMyInvoicesAsync();

    // null si la factura no está pagada, no le pertenece al caller, o falla la red -- la
    // pantalla trata los tres casos igual: no se puede descargar ahora mismo.
    Task<byte[]?> DownloadReceiptAsync(Guid invoiceId);
}
