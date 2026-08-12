using IrrigationMaster.Mobile.Application.Features.Models.Notifications;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.Notifications;
using IrrigationMaster.UI.Maui.Tests.TestDoubles;

namespace IrrigationMaster.UI.Maui.Tests;

public class NotificationsViewModelTests
{
    private static readonly Guid UnreadNotificationId = Guid.NewGuid();
    private static readonly Guid ReadNotificationId = Guid.NewGuid();

    private static (NotificationsViewModel ViewModel, FakeNotificationService NotificationService, RecordingAlertService Alerts) CreateSut(
        List<NotificationDto>? notificationsToReturn = null)
    {
        var notificationService = new FakeNotificationService { NotificationsToReturn = notificationsToReturn ?? [] };
        var alerts = new RecordingAlertService();

        var viewModel = new NotificationsViewModel(notificationService, alerts);

        return (viewModel, notificationService, alerts);
    }

    // ─── CARGA ───

    [Fact]
    public async Task LoadAsync_PopulatesNotifications_PreservingBackendOrder()
    {
        // El backend ya devuelve la lista ordenada por Created descendente -- el ViewModel no
        // debe reordenar nada.
        var newer = new NotificationDto { Id = Guid.NewGuid(), Title = "Más reciente", Message = "Msg A", IsRead = false, Created = new DateTime(2026, 8, 12, 10, 0, 0, DateTimeKind.Utc) };
        var older = new NotificationDto { Id = Guid.NewGuid(), Title = "Más antigua", Message = "Msg B", IsRead = true, Created = new DateTime(2026, 8, 10, 10, 0, 0, DateTimeKind.Utc) };
        var (vm, _, _) = CreateSut([newer, older]);

        await vm.LoadAsync();

        Assert.Equal(2, vm.Notifications.Count);
        Assert.Equal("Más reciente", vm.Notifications[0].Title);
        Assert.Equal("Más antigua", vm.Notifications[1].Title);
    }

    [Fact]
    public async Task LoadAsync_MapsReadAndUnreadFlagsCorrectly()
    {
        var unread = new NotificationDto { Id = UnreadNotificationId, Title = "Sin leer", Message = "Msg", IsRead = false, Created = DateTime.UtcNow };
        var read = new NotificationDto { Id = ReadNotificationId, Title = "Leída", Message = "Msg", IsRead = true, Created = DateTime.UtcNow };
        var (vm, _, _) = CreateSut([unread, read]);

        await vm.LoadAsync();

        var unreadItem = vm.Notifications.Single(n => n.Id == UnreadNotificationId);
        var readItem = vm.Notifications.Single(n => n.Id == ReadNotificationId);
        Assert.True(unreadItem.IsUnread);
        Assert.False(readItem.IsUnread);
    }

    // ─── MARCAR UNA (al tocarla) ───

    [Fact]
    public async Task SelectNotificationAsync_WhenUnread_MarksAsRead_AndReloads()
    {
        var (vm, notificationService, _) = CreateSut();
        var notification = new NotificationItem { Id = UnreadNotificationId, Title = "T", Message = "M", IsRead = false, Created = DateTime.UtcNow };

        await vm.SelectNotificationAsync(notification);

        Assert.Equal(UnreadNotificationId, notificationService.LastMarkAsReadCall);
    }

    [Fact]
    public async Task SelectNotificationAsync_WhenAlreadyRead_DoesNotCallMarkAsRead()
    {
        var (vm, notificationService, _) = CreateSut();
        var notification = new NotificationItem { Id = ReadNotificationId, Title = "T", Message = "M", IsRead = true, Created = DateTime.UtcNow };

        await vm.SelectNotificationAsync(notification);

        Assert.Null(notificationService.LastMarkAsReadCall);
    }

    [Fact]
    public async Task SelectNotificationAsync_WithNullNotification_DoesNothing()
    {
        var (vm, notificationService, alerts) = CreateSut();

        await vm.SelectNotificationAsync(null);

        Assert.Null(notificationService.LastMarkAsReadCall);
        Assert.Empty(alerts.Calls);
    }

    [Fact]
    public async Task SelectNotificationAsync_WhenBackendRejects_ShowsExactBackendMessage()
    {
        var (vm, notificationService, alerts) = CreateSut();
        notificationService.MarkAsReadResult = new UserActionResult
        {
            IsSuccess = false,
            Message = "Operación rechazada: No existe ningún registro de 'Notification' con el ID indicado."
        };
        var notification = new NotificationItem { Id = UnreadNotificationId, Title = "T", Message = "M", IsRead = false, Created = DateTime.UtcNow };

        await vm.SelectNotificationAsync(notification);

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal("Operación rechazada: No existe ningún registro de 'Notification' con el ID indicado.", alert.Message);
    }

    // ─── MARCAR TODAS ───

    [Fact]
    public async Task MarkAllAsReadAsync_OnSuccess_ShowsSuccessAlert_AndReloads()
    {
        var (vm, notificationService, alerts) = CreateSut();

        await vm.MarkAllAsReadAsync();

        Assert.Equal(1, notificationService.MarkAllAsReadCallCount);
        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.SuccessTitle, alert.Title);
        Assert.Equal(AppStrings.NotificationsMarkedAllReadSuccess, alert.Message);
    }

    [Fact]
    public async Task MarkAllAsReadAsync_WhenBackendRejects_ShowsExactBackendMessage()
    {
        var (vm, notificationService, alerts) = CreateSut();
        notificationService.MarkAllAsReadResult = new UserActionResult
        {
            IsSuccess = false,
            Message = "Fallo de conexión con el servidor."
        };

        await vm.MarkAllAsReadAsync();

        var alert = Assert.Single(alerts.Calls);
        Assert.Equal(AppStrings.ErrorTitle, alert.Title);
        Assert.Equal("Fallo de conexión con el servidor.", alert.Message);
    }
}
