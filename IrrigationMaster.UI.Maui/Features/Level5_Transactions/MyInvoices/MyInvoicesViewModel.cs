using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.UI.Maui.Common;

namespace IrrigationMaster.UI.Maui.Features.Level5_Transactions.MyInvoices;

// Una factura de la lista. Plantilla plana (no ObservableObject), mismo criterio que
// NotificationItem: se reconstruye entera en cada LoadAsync.
public class InvoiceItem
{
    public Guid Id { get; init; }
    public string InvoiceNumber { get; init; } = string.Empty;
    public DateTime IssueDate { get; init; }
    public DateTime DueDate { get; init; }
    public decimal TotalAmountValue { get; init; }
    public string TotalAmountCurrency { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public bool IsPaid { get; init; }

    public string IssueDateDisplay => IssueDate.ToLocalTime().ToString("dd/MM/yyyy");
    public string AmountDisplay => $"{TotalAmountValue:N2} {TotalAmountCurrency}";
}

/// <summary>
/// "Mis Facturas": licencia individual del propio usuario logueado (Invoices/MyInvoices),
/// visible para los 3 roles sin restricción -- autoservicio puro, igual que Notificaciones. Sin
/// relación con la factura/licencia colectiva de la organización, que esta App no expone.
/// </summary>
public partial class MyInvoicesViewModel : ObservableObject
{
    private readonly IInvoiceService _invoiceService;
    private readonly IAlertService _alertService;
    private readonly IReceiptOpener _receiptOpener;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<InvoiceItem> Invoices { get; } = [];

    public MyInvoicesViewModel(IInvoiceService invoiceService, IAlertService alertService, IReceiptOpener receiptOpener)
    {
        _invoiceService = invoiceService;
        _alertService = alertService;
        _receiptOpener = receiptOpener;
    }

    [RelayCommand]
    internal async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _invoiceService.GetMyInvoicesAsync();

            Invoices.Clear();
            foreach (var invoice in list ?? [])
            {
                Invoices.Add(new InvoiceItem
                {
                    Id = invoice.Id,
                    InvoiceNumber = invoice.InvoiceNumber,
                    IssueDate = invoice.IssueDate,
                    DueDate = invoice.DueDate,
                    TotalAmountValue = invoice.TotalAmountValue,
                    TotalAmountCurrency = invoice.TotalAmountCurrency,
                    Status = invoice.Status,
                    IsPaid = invoice.IsPaid
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Loading MyInvoices]: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Descarga el PDF a un fichero temporal y deja que el propio sistema operativo lo abra con el
    // visor de PDF que tenga instalado -- no hay precedente de descarga de fichero en esta App, y
    // así evitamos implementar un visor propio para un caso de uso tan puntual.
    [RelayCommand]
    internal async Task DownloadReceiptAsync(InvoiceItem? invoice)
    {
        if (invoice is null || !invoice.IsPaid || IsBusy) return;

        IsBusy = true;
        try
        {
            var bytes = await _invoiceService.DownloadReceiptAsync(invoice.Id);
            if (bytes is null)
            {
                await _alertService.ShowAsync(AppStrings.ErrorTitle, AppStrings.InvoiceReceiptDownloadError);
                return;
            }

            await _receiptOpener.OpenAsync($"comprobante-{invoice.InvoiceNumber}.pdf", bytes);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Downloading Receipt]: {ex.Message}");
            await _alertService.ShowAsync(AppStrings.ErrorTitle, AppStrings.InvoiceReceiptDownloadError);
        }
        finally
        {
            IsBusy = false;
        }
    }
}
