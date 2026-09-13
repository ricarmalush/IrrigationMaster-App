using IrrigationMaster.Mobile.Application.Features.Models.Invoices;
using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class FakeInvoiceService : IInvoiceService
{
    public List<InvoiceDto>? InvoicesToReturn { get; set; } = [];
    public byte[]? ReceiptBytesToReturn { get; set; } = [1, 2, 3];
    public bool GetMyInvoicesCalled { get; private set; }
    public Guid? LastDownloadReceiptCall { get; private set; }

    public Task<List<InvoiceDto>?> GetMyInvoicesAsync()
    {
        GetMyInvoicesCalled = true;
        return Task.FromResult(InvoicesToReturn);
    }

    public Task<byte[]?> DownloadReceiptAsync(Guid invoiceId)
    {
        LastDownloadReceiptCall = invoiceId;
        return Task.FromResult(ReceiptBytesToReturn);
    }
}
