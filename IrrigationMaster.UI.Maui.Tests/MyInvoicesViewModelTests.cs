using IrrigationMaster.Mobile.Application.Features.Models.Invoices;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level5_Transactions.MyInvoices;
using IrrigationMaster.UI.Maui.Tests.TestDoubles;

namespace IrrigationMaster.UI.Maui.Tests;

public class MyInvoicesViewModelTests
{
    private static (MyInvoicesViewModel ViewModel, FakeInvoiceService Invoices, RecordingAlertService Alerts, FakeReceiptOpener Opener) CreateSut()
    {
        var invoices = new FakeInvoiceService();
        var alerts = new RecordingAlertService();
        var opener = new FakeReceiptOpener();

        var viewModel = new MyInvoicesViewModel(invoices, alerts, opener);

        return (viewModel, invoices, alerts, opener);
    }

    [Fact]
    public async Task LoadAsync_PopulatesInvoices_FromTheService()
    {
        var (vm, invoices, _, _) = CreateSut();
        invoices.InvoicesToReturn =
        [
            new InvoiceDto { Id = Guid.NewGuid(), InvoiceNumber = "INV-001", TotalAmountValue = 19.90m, TotalAmountCurrency = "EUR", Status = "Paid" },
            new InvoiceDto { Id = Guid.NewGuid(), InvoiceNumber = "INV-002", TotalAmountValue = 9.90m, TotalAmountCurrency = "EUR", Status = "Issued" }
        ];

        await vm.LoadAsync();

        Assert.False(vm.IsBusy);
        Assert.Equal(2, vm.Invoices.Count);
        Assert.Equal("INV-001", vm.Invoices[0].InvoiceNumber);
        Assert.True(vm.Invoices[0].IsPaid);
        Assert.False(vm.Invoices[1].IsPaid);
    }

    [Fact]
    public async Task LoadAsync_WhenServiceReturnsNull_LeavesInvoicesEmpty_WithoutThrowing()
    {
        var (vm, invoices, _, _) = CreateSut();
        invoices.InvoicesToReturn = null;

        await vm.LoadAsync();

        Assert.False(vm.IsBusy);
        Assert.Empty(vm.Invoices);
    }

    [Fact]
    public async Task DownloadReceiptAsync_ForAPaidInvoice_CallsTheOpener_WithTheReceiptBytes()
    {
        var (vm, invoices, alerts, opener) = CreateSut();
        invoices.ReceiptBytesToReturn = [9, 8, 7];
        var invoiceId = Guid.NewGuid();
        var item = new InvoiceItem { Id = invoiceId, InvoiceNumber = "INV-001", IsPaid = true };

        await vm.DownloadReceiptAsync(item);

        Assert.Equal(invoiceId, invoices.LastDownloadReceiptCall);
        var call = Assert.Single(opener.Calls);
        Assert.Equal("comprobante-INV-001.pdf", call.FileName);
        Assert.Equal(new byte[] { 9, 8, 7 }, call.Content);
        Assert.Empty(alerts.Calls);
    }

    [Fact]
    public async Task DownloadReceiptAsync_ForANonPaidInvoice_DoesNothing()
    {
        var (vm, invoices, _, opener) = CreateSut();
        var item = new InvoiceItem { Id = Guid.NewGuid(), InvoiceNumber = "INV-002", IsPaid = false };

        await vm.DownloadReceiptAsync(item);

        Assert.Null(invoices.LastDownloadReceiptCall);
        Assert.Empty(opener.Calls);
    }

    [Fact]
    public async Task DownloadReceiptAsync_WhenServiceReturnsNull_ShowsAnAlert_WithoutCallingTheOpener()
    {
        var (vm, invoices, alerts, opener) = CreateSut();
        invoices.ReceiptBytesToReturn = null;
        var item = new InvoiceItem { Id = Guid.NewGuid(), InvoiceNumber = "INV-001", IsPaid = true };

        await vm.DownloadReceiptAsync(item);

        Assert.Empty(opener.Calls);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal(AppStrings.InvoiceReceiptDownloadError, alert.Message);
    }

    [Fact]
    public async Task DownloadReceiptAsync_WhenTheOpenerThrows_ShowsAnAlert_InsteadOfPropagating()
    {
        var (vm, _, alerts, opener) = CreateSut();
        opener.ExceptionToThrow = new InvalidOperationException("no hay visor de PDF instalado");
        var item = new InvoiceItem { Id = Guid.NewGuid(), InvoiceNumber = "INV-001", IsPaid = true };

        await vm.DownloadReceiptAsync(item);

        Assert.False(vm.IsBusy);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.InvoiceReceiptDownloadError, alert.Message);
    }
}
