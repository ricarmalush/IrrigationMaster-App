using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.UI.Maui.Common;
using System.Collections.ObjectModel;

namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.Notifications;

// Una notificación de la lista. Plantilla plana (no ObservableObject): se reconstruye entera en
// cada LoadAsync, no hay estado mutable propio que necesite notificar cambios -- marcar como
// leída dispara una recarga completa, igual que Start/CompleteTurn en IrrigationStatusViewModel.
public class NotificationItem
{
    public Guid Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public bool IsRead { get; init; }
    public DateTime Created { get; init; }

    public bool IsUnread => !IsRead;
    public string CreatedDisplay => Created.ToLocalTime().ToString("dd/MM/yyyy HH:mm");
}

/// <summary>
/// Notificaciones del propio usuario logueado, visible para los 3 roles sin restricción. El
/// backend ya devuelve la lista ordenada por fecha descendente (más reciente primero), no hace
/// falta ordenar aquí.
/// </summary>
public partial class NotificationsViewModel : ObservableObject
{
    private readonly INotificationService _notificationService;
    private readonly IAlertService _alertService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<NotificationItem> Notifications { get; } = [];

    public NotificationsViewModel(INotificationService notificationService, IAlertService alertService)
    {
        _notificationService = notificationService;
        _alertService = alertService;
    }

    [RelayCommand]
    internal async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            var list = await _notificationService.GetMyNotificationsAsync();

            Notifications.Clear();
            foreach (var notification in list ?? [])
            {
                Notifications.Add(new NotificationItem
                {
                    Id = notification.Id,
                    Title = notification.Title,
                    Message = notification.Message,
                    IsRead = notification.IsRead,
                    Created = notification.Created
                });
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Loading Notifications]: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    // Al tocar una notificación: si ya está leída, no hace nada (evita una llamada de red
    // innecesaria); si no está leída, la marca automáticamente y recarga la lista.
    [RelayCommand]
    internal async Task SelectNotificationAsync(NotificationItem? notification)
    {
        if (notification is null || notification.IsRead) return;

        var result = await _notificationService.MarkAsReadAsync(notification.Id);
        if (result.IsSuccess)
        {
            await LoadAsync();
        }
        else
        {
            await _alertService.ShowAsync(AppStrings.ErrorTitle, BuildFailureMessage(result));
        }
    }

    [RelayCommand]
    internal async Task MarkAllAsReadAsync()
    {
        var result = await _notificationService.MarkAllAsReadAsync();
        if (result.IsSuccess)
        {
            await _alertService.ShowAsync(AppStrings.SuccessTitle, AppStrings.NotificationsMarkedAllReadSuccess);
            await LoadAsync();
        }
        else
        {
            await _alertService.ShowAsync(AppStrings.ErrorTitle, BuildFailureMessage(result));
        }
    }

    private static string BuildFailureMessage(UserActionResult result)
    {
        if (result.Errors is { Count: > 0 })
            return string.Join("\n", result.Errors.Select(e => $"{e.PropertyMessage}: {e.ErrorMessage}"));

        return string.IsNullOrWhiteSpace(result.Message) ? AppStrings.ApiConnectionError : result.Message;
    }
}
