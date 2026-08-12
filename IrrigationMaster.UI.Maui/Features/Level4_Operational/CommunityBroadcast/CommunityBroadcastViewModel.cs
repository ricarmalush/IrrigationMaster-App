using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrrigationMaster.Mobile.Application.Features.Models.Notifications;
using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.UI.Maui.Common;
using System.Collections.ObjectModel;

namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.CommunityBroadcast;

/// <summary>
/// "Avisar a mi comunidad" (Presidente/SUPERADMIN, permiso SEND_NOTIFICATIONS -- el gating de
/// visibilidad del botón vive en AdminMenuPage, no aquí). Ofrece dos audiencias posibles:
/// "Mi andador" (solo si el propio llamador tiene un andador asignado -- el JWT no lleva ese dato,
/// así que se resuelve con Users/Get/{id} sobre el propio CachedUserId) y "Toda mi organización"
/// (siempre disponible). El backend resuelve la organización/andador del destinatario desde el
/// propio llamador, nunca desde el body -- por eso no hay selector de organización aquí.
/// </summary>
public partial class CommunityBroadcastViewModel : ObservableObject
{
    internal const string WalkwayAudienceLabel = "Mi andador";
    internal const string OrganizationAudienceLabel = "Toda mi organización";

    private readonly INotificationService _notificationService;
    private readonly IUserManagementService _userManagementService;
    private readonly ICurrentSession _currentSession;
    private readonly IAlertService _alertService;

    private Guid? _myWalkwayId;

    [ObservableProperty] public partial string Message { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(AudienceSelectionDisplay))]
    public partial string? SelectedAudience { get; set; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotBusy))]
    public partial bool IsBusy { get; set; }

    public bool IsNotBusy => !IsBusy;

    public ObservableCollection<string> AudienceOptions { get; } = [];

    // Picker en Windows no refresca su texto visible al cambiar SelectedItem (bug conocido,
    // dotnet/maui #5038/#24369); mismo patrón que RoleSelectionDisplay/WalkwaySelectionDisplay en
    // UserManagementViewModel.
    public string AudienceSelectionDisplay =>
        string.IsNullOrWhiteSpace(SelectedAudience) ? "Sin seleccionar" : SelectedAudience;

    public CommunityBroadcastViewModel(
        INotificationService notificationService,
        IUserManagementService userManagementService,
        ICurrentSession currentSession,
        IAlertService alertService)
    {
        _notificationService = notificationService;
        _userManagementService = userManagementService;
        _currentSession = currentSession;
        _alertService = alertService;
    }

    [RelayCommand]
    internal async Task LoadAsync()
    {
        IsBusy = true;
        try
        {
            AudienceOptions.Clear();
            _myWalkwayId = null;

            var userId = _currentSession.CachedUserId;
            if (userId.HasValue)
            {
                var me = await _userManagementService.GetUserByIdAsync(userId.Value);
                _myWalkwayId = me?.WalkwayId;
            }

            if (_myWalkwayId.HasValue)
                AudienceOptions.Add(WalkwayAudienceLabel);
            AudienceOptions.Add(OrganizationAudienceLabel);

            SelectedAudience = AudienceOptions.FirstOrDefault();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Loading CommunityBroadcast]: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    internal async Task SendAsync()
    {
        if (string.IsNullOrWhiteSpace(Message))
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgMissingBroadcastMessage);
            return;
        }

        if (string.IsNullOrWhiteSpace(SelectedAudience))
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgSelectAudienceFirst);
            return;
        }

        IsBusy = true;
        try
        {
            var isWalkwayAudience = SelectedAudience == WalkwayAudienceLabel;
            var audience = isWalkwayAudience ? "Walkway" : "Organization";
            var targetWalkwayId = isWalkwayAudience ? _myWalkwayId : null;

            var result = await _notificationService.SendNotificationAsync(audience, Message.Trim(), targetWalkwayId);

            if (result.IsSuccess)
            {
                Message = string.Empty;
                await _alertService.ShowAsync(AppStrings.SuccessTitle,
                    string.Format(AppStrings.NotificationSentSuccessFormat, result.RecipientCount));
            }
            else
            {
                await _alertService.ShowAsync(AppStrings.ErrorTitle, BuildFailureMessage(result));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error SendNotification]: {ex.Message}");
            await _alertService.ShowAsync(AppStrings.ErrorTitle, AppStrings.ApiConnectionError);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string BuildFailureMessage(SendNotificationResult result)
    {
        if (result.Errors is { Count: > 0 })
            return string.Join("\n", result.Errors.Select(e => $"{e.PropertyMessage}: {e.ErrorMessage}"));

        return string.IsNullOrWhiteSpace(result.Message) ? AppStrings.ApiConnectionError : result.Message;
    }
}
