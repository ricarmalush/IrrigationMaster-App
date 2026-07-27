namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.AdminConsole;

public partial class SystemSettingsPage : TabbedPage
{
    public SystemSettingsPage(SystemSettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}