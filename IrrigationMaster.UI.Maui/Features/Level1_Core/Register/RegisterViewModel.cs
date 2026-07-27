using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrrigationMaster.Mobile.Application.Common.Dtos;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.Mobile.Infrastructure;
using IrrigationMaster.UI.Maui.Common;

namespace IrrigationMaster.UI.Maui.Features.Level1_Core.Register;

public partial class RegisterViewModel : ObservableObject
{
    // Longitud mínima del código de invitación antes de intentar la llamada de red -- el formato
    // real (alfabeto, existencia) siempre lo valida el backend; esto solo evita un viaje de red
    // inútil con un campo vacío o claramente incompleto.
    private const int MinInvitationCodeLength = 8;

    private readonly IRegistrationService _registrationService;
    private readonly IAlertService _alertService;
    private readonly INavigationService _navigationService;

    [ObservableProperty] public partial string FirstName { get; set; } = string.Empty;
    [ObservableProperty] public partial string LastName { get; set; } = string.Empty;
    [ObservableProperty] public partial string Email { get; set; } = string.Empty;
    [ObservableProperty] public partial string Password { get; set; } = string.Empty;
    [ObservableProperty] public partial string InvitationCode { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsBusy { get; set; }

    public RegisterViewModel(IRegistrationService registrationService, IAlertService alertService, INavigationService navigationService)
    {
        _registrationService = registrationService;
        _alertService = alertService;
        _navigationService = navigationService;
    }

    [RelayCommand]
    internal async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) ||
            string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password) ||
            string.IsNullOrWhiteSpace(InvitationCode))
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgMissingRegisterData);
            return;
        }

        if (InvitationCode.Trim().Length < MinInvitationCodeLength)
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgInvalidInvitationCode);
            return;
        }

        IsBusy = true;
        try
        {
            // WalkwayId queda sin asignar (es opcional en CreateUserCommand): la asignación de
            // andador pasa a ser responsabilidad del Presidente al aprobar al usuario, no del
            // propio registro anónimo.
            var result = await _registrationService.RegisterAsync(new CreateUserRequest
            {
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                Email = Email.Trim(),
                Password = Password,
                InvitationCode = InvitationCode.Trim(),
                RoleId = TenantConfig.DefaultVecinoRoleId
            });

            if (result.IsSuccess)
            {
                await _alertService.ShowAsync(AppStrings.SuccessTitle, AppStrings.UserRegisteredSuccess);
                await _navigationService.GoToAsync("..");
            }
            else
            {
                await _alertService.ShowAsync(AppStrings.ErrorTitle, BuildFailureMessage(result));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Register]: {ex.Message}");
            await _alertService.ShowAsync(AppStrings.ErrorTitle, AppStrings.ApiConnectionError);
        }
        finally
        {
            IsBusy = false;
        }
    }

    private static string BuildFailureMessage(StructureOperationResult result)
    {
        if (result.Errors is { Count: > 0 })
            return string.Join("\n", result.Errors.Select(e => $"{e.PropertyMessage}: {e.ErrorMessage}"));

        return string.IsNullOrWhiteSpace(result.Message) ? AppStrings.ApiConnectionError : result.Message;
    }
}
