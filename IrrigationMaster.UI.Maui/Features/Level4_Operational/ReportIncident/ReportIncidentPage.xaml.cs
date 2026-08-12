namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.ReportIncident;

public partial class ReportIncidentPage : ContentPage
{
    public ReportIncidentPage(ReportIncidentViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}
