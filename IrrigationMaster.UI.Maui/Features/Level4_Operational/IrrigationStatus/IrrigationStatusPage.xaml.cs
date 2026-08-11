namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.IrrigationStatus;

public partial class IrrigationStatusPage : ContentPage
{
    private readonly IrrigationStatusViewModel _viewModel;

    public IrrigationStatusPage(IrrigationStatusViewModel viewModel)
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
