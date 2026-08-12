namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.CommunityBroadcast;

public partial class CommunityBroadcastPage : ContentPage
{
    private readonly CommunityBroadcastViewModel _viewModel;

    public CommunityBroadcastPage(CommunityBroadcastViewModel viewModel)
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
