using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.ReportIncident;
using IrrigationMaster.UI.Maui.Tests.TestDoubles;

namespace IrrigationMaster.UI.Maui.Tests;

public class ReportIncidentViewModelTests
{
    private static (ReportIncidentViewModel ViewModel, FakeNotificationService NotificationService, RecordingAlertService Alerts) CreateSut()
    {
        var notificationService = new FakeNotificationService();
        var alerts = new RecordingAlertService();

        var viewModel = new ReportIncidentViewModel(notificationService, alerts);

        return (viewModel, notificationService, alerts);
    }

    [Fact]
    public async Task SendAsync_WithEmptyDescription_ShowsValidationWarning_WithoutCallingApi()
    {
        var (vm, notificationService, alerts) = CreateSut();
        vm.Description = "   ";

        await vm.SendCommand.ExecuteAsync(null);

        Assert.Null(notificationService.LastReportIncidentMessage);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.AttentionTitle, alert.Title);
        Assert.Equal(AppStrings.MsgMissingIncidentDescription, alert.Message);
    }

    [Fact]
    public async Task SendAsync_WithDescription_CallsApi_WithTrimmedMessage()
    {
        var (vm, notificationService, _) = CreateSut();
        vm.Description = "  Fuga de agua en el andador 3  ";

        await vm.SendCommand.ExecuteAsync(null);

        Assert.Equal("Fuga de agua en el andador 3", notificationService.LastReportIncidentMessage);
    }

    [Fact]
    public async Task SendAsync_OnSuccess_ShowsConfirmation_AndClearsDescription()
    {
        var (vm, _, alerts) = CreateSut();
        vm.Description = "Fuga de agua en el andador 3";

        await vm.SendCommand.ExecuteAsync(null);

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.SuccessTitle, alert.Title);
        Assert.Equal(AppStrings.IncidentReportedSuccess, alert.Message);
        Assert.Equal(string.Empty, vm.Description);
    }

    [Fact]
    public async Task SendAsync_WhenBackendRejects_ShowsExactBackendMessage_AndKeepsDescription()
    {
        var (vm, notificationService, alerts) = CreateSut();
        notificationService.ReportIncidentResult = new IrrigationMaster.Mobile.Application.Features.Models.Users.UserActionResult
        {
            IsSuccess = false,
            Message = "Operación rechazada: no tienes permiso para reportar incidencias."
        };
        vm.Description = "Fuga de agua en el andador 3";

        await vm.SendCommand.ExecuteAsync(null);

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal("Operación rechazada: no tienes permiso para reportar incidencias.", alert.Message);
        Assert.Equal("Fuga de agua en el andador 3", vm.Description);
    }

    [Fact]
    public async Task SendAsync_TogglesIsBusy_DuringExecution()
    {
        var (vm, _, _) = CreateSut();
        vm.Description = "Fuga de agua en el andador 3";

        Assert.True(vm.IsNotBusy);
        var task = vm.SendCommand.ExecuteAsync(null);
        await task;

        Assert.False(vm.IsBusy);
        Assert.True(vm.IsNotBusy);
    }
}
