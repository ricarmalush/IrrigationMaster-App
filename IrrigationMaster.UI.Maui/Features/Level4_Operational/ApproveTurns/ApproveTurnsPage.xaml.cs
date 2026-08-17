namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.ApproveTurns;

public partial class ApproveTurnsPage : ContentPage
{
    private readonly ApproveTurnsViewModel _viewModel;

    public ApproveTurnsPage(ApproveTurnsViewModel viewModel)
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
