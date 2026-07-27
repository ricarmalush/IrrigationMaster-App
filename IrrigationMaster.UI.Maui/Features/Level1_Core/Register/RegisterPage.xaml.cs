namespace IrrigationMaster.UI.Maui.Features.Level1_Core.Register;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}