using IrrigationMaster.Mobile.Application.Features.Models.Notifications;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.CommunityBroadcast;
using IrrigationMaster.UI.Maui.Tests.TestDoubles;

namespace IrrigationMaster.UI.Maui.Tests;

public class CommunityBroadcastViewModelTests
{
    private static readonly Guid CurrentUserId = Guid.NewGuid();
    private static readonly Guid MyWalkwayId = Guid.NewGuid();

    private static (CommunityBroadcastViewModel ViewModel, FakeNotificationService NotificationService, FakeUserManagementService UserManagementService, RecordingAlertService Alerts) CreateSut(
        AppUserDto? currentUser = null)
    {
        var notificationService = new FakeNotificationService();
        var userManagementService = new FakeUserManagementService { UserByIdToReturn = currentUser };
        var currentSession = new FakeCurrentSession { UserIdToReturn = CurrentUserId };
        var alerts = new RecordingAlertService();

        var viewModel = new CommunityBroadcastViewModel(notificationService, userManagementService, currentSession, alerts);

        return (viewModel, notificationService, userManagementService, alerts);
    }

    // ─── SELECCIÓN DE AUDIENCIA ───

    [Fact]
    public async Task LoadAsync_WhenCallerHasWalkwayAssigned_OffersBothAudiences_DefaultingToWalkway()
    {
        var (vm, _, userManagementService, _) = CreateSut(new AppUserDto { Id = CurrentUserId, WalkwayId = MyWalkwayId, WalkwayCode = "A-01" });

        await vm.LoadAsync();

        Assert.Equal(CurrentUserId, userManagementService.LastGetUserByIdCall);
        Assert.Equal([CommunityBroadcastViewModel.WalkwayAudienceLabel, CommunityBroadcastViewModel.OrganizationAudienceLabel], vm.AudienceOptions);
        Assert.Equal(CommunityBroadcastViewModel.WalkwayAudienceLabel, vm.SelectedAudience);
    }

    [Fact]
    public async Task LoadAsync_WhenCallerHasNoWalkwayAssigned_OffersOnlyOrganizationAudience()
    {
        var (vm, _, _, _) = CreateSut(new AppUserDto { Id = CurrentUserId, WalkwayId = null });

        await vm.LoadAsync();

        Assert.Equal([CommunityBroadcastViewModel.OrganizationAudienceLabel], vm.AudienceOptions);
        Assert.Equal(CommunityBroadcastViewModel.OrganizationAudienceLabel, vm.SelectedAudience);
    }

    // ─── VALIDACIÓN LOCAL ───

    [Fact]
    public async Task SendAsync_WithEmptyMessage_ShowsValidationWarning_WithoutCallingApi()
    {
        var (vm, notificationService, _, alerts) = CreateSut(new AppUserDto { Id = CurrentUserId, WalkwayId = null });
        await vm.LoadAsync();
        vm.Message = "   ";

        await vm.SendCommand.ExecuteAsync(null);

        Assert.Null(notificationService.LastSendNotificationCall);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.AttentionTitle, alert.Title);
        Assert.Equal(AppStrings.MsgMissingBroadcastMessage, alert.Message);
    }

    [Fact]
    public async Task SendAsync_WithoutSelectedAudience_ShowsValidationWarning_WithoutCallingApi()
    {
        var (vm, notificationService, _, alerts) = CreateSut(new AppUserDto { Id = CurrentUserId, WalkwayId = null });
        await vm.LoadAsync();
        vm.SelectedAudience = null;
        vm.Message = "Corte de agua mañana";

        await vm.SendCommand.ExecuteAsync(null);

        Assert.Null(notificationService.LastSendNotificationCall);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.AttentionTitle, alert.Title);
        Assert.Equal(AppStrings.MsgSelectAudienceFirst, alert.Message);
    }

    // ─── ENVÍO SEGÚN AUDIENCIA ───

    [Fact]
    public async Task SendAsync_WhenOrganizationAudienceSelected_CallsApi_WithOrganizationAudience_AndNoWalkwayId()
    {
        var (vm, notificationService, _, _) = CreateSut(new AppUserDto { Id = CurrentUserId, WalkwayId = MyWalkwayId });
        await vm.LoadAsync();
        vm.SelectedAudience = CommunityBroadcastViewModel.OrganizationAudienceLabel;
        vm.Message = "  Corte de agua mañana  ";

        await vm.SendCommand.ExecuteAsync(null);

        var call = notificationService.LastSendNotificationCall;
        Assert.NotNull(call);
        Assert.Equal("Organization", call!.Value.Audience);
        Assert.Equal("Corte de agua mañana", call.Value.Message);
        Assert.Null(call.Value.TargetWalkwayId);
    }

    [Fact]
    public async Task SendAsync_WhenWalkwayAudienceSelected_CallsApi_WithWalkwayAudience_AndOwnWalkwayId()
    {
        var (vm, notificationService, _, _) = CreateSut(new AppUserDto { Id = CurrentUserId, WalkwayId = MyWalkwayId });
        await vm.LoadAsync();
        vm.SelectedAudience = CommunityBroadcastViewModel.WalkwayAudienceLabel;
        vm.Message = "Riego suspendido hoy";

        await vm.SendCommand.ExecuteAsync(null);

        var call = notificationService.LastSendNotificationCall;
        Assert.NotNull(call);
        Assert.Equal("Walkway", call!.Value.Audience);
        Assert.Equal(MyWalkwayId, call.Value.TargetWalkwayId);
    }

    // ─── CONFIRMACIÓN CON CONTEO ───

    [Fact]
    public async Task SendAsync_OnSuccess_ShowsConfirmationWithRecipientCount_AndClearsMessage()
    {
        var (vm, notificationService, _, alerts) = CreateSut(new AppUserDto { Id = CurrentUserId, WalkwayId = null });
        notificationService.SendNotificationResult = new SendNotificationResult { IsSuccess = true, RecipientCount = 7 };
        await vm.LoadAsync();
        vm.Message = "Corte de agua mañana";

        await vm.SendCommand.ExecuteAsync(null);

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.SuccessTitle, alert.Title);
        Assert.Equal("Aviso enviado a 7 destinatarios.", alert.Message);
        Assert.Equal(string.Empty, vm.Message);
    }

    [Fact]
    public async Task SendAsync_WhenBackendRejects_ShowsExactBackendMessage_AndKeepsMessage()
    {
        var (vm, notificationService, _, alerts) = CreateSut(new AppUserDto { Id = CurrentUserId, WalkwayId = null });
        notificationService.SendNotificationResult = new SendNotificationResult
        {
            IsSuccess = false,
            Message = "Operación rechazada: no tienes permiso para enviar notificaciones."
        };
        await vm.LoadAsync();
        vm.Message = "Corte de agua mañana";

        await vm.SendCommand.ExecuteAsync(null);

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal("Operación rechazada: no tienes permiso para enviar notificaciones.", alert.Message);
        Assert.Equal("Corte de agua mañana", vm.Message);
    }
}
