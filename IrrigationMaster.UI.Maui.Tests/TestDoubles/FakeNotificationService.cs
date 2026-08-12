using IrrigationMaster.Mobile.Application.Features.Models.Notifications;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class FakeNotificationService : INotificationService
{
    public List<NotificationDto>? NotificationsToReturn { get; set; } = [];
    public UserActionResult MarkAsReadResult { get; set; } = new() { IsSuccess = true };
    public UserActionResult MarkAllAsReadResult { get; set; } = new() { IsSuccess = true };
    public UserActionResult ReportIncidentResult { get; set; } = new() { IsSuccess = true };
    public SendNotificationResult SendNotificationResult { get; set; } = new() { IsSuccess = true, RecipientCount = 0 };

    public Guid? LastMarkAsReadCall { get; private set; }
    public int MarkAllAsReadCallCount { get; private set; }
    public string? LastReportIncidentMessage { get; private set; }
    public (string Audience, string Message, Guid? TargetWalkwayId)? LastSendNotificationCall { get; private set; }

    public Task<List<NotificationDto>?> GetMyNotificationsAsync() => Task.FromResult(NotificationsToReturn);

    public Task<UserActionResult> MarkAsReadAsync(Guid notificationId)
    {
        LastMarkAsReadCall = notificationId;
        return Task.FromResult(MarkAsReadResult);
    }

    public Task<UserActionResult> MarkAllAsReadAsync()
    {
        MarkAllAsReadCallCount++;
        return Task.FromResult(MarkAllAsReadResult);
    }

    public Task<UserActionResult> ReportIncidentAsync(string message)
    {
        LastReportIncidentMessage = message;
        return Task.FromResult(ReportIncidentResult);
    }

    public Task<SendNotificationResult> SendNotificationAsync(string audience, string message, Guid? targetWalkwayId)
    {
        LastSendNotificationCall = (audience, message, targetWalkwayId);
        return Task.FromResult(SendNotificationResult);
    }
}
