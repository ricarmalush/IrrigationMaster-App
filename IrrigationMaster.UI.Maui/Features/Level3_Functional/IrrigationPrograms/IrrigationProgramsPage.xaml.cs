namespace IrrigationMaster.UI.Maui.Features.Level3_Functional.IrrigationPrograms;

public partial class IrrigationProgramsPage : ContentPage
{
    private readonly IrrigationProgramsViewModel _viewModel;

    public IrrigationProgramsPage(IrrigationProgramsViewModel viewModel)
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
