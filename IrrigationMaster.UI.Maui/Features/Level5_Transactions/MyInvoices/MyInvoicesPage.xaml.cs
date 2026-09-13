namespace IrrigationMaster.UI.Maui.Features.Level5_Transactions.MyInvoices;

public partial class MyInvoicesPage : ContentPage
{
    private readonly MyInvoicesViewModel _viewModel;

    public MyInvoicesPage(MyInvoicesViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _viewModel.LoadAsync();
    }
}
