using Microsoft.Maui.Controls;

namespace IrrigationMaster.UI.Maui.Features.Level1_Core.Login;

public partial class MainPage : ContentPage
{
    // 🟢 Acoplamiento profesional: la vista recibe su cerebro listo mediante inyección
    public MainPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}