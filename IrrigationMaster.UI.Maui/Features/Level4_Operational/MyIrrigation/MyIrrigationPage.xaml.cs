namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.MyIrrigation;

public partial class MyIrrigationPage : ContentPage
{
    private readonly MyIrrigationViewModel _viewModel;

    public MyIrrigationPage(MyIrrigationViewModel viewModel)
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
