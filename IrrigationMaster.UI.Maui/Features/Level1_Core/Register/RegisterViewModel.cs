using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using IrrigationMaster.Mobile.Application.Common.Dtos;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.Mobile.Infrastructure;
using IrrigationMaster.UI.Maui.Common;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace IrrigationMaster.UI.Maui.Features.Level1_Core.Register;

// Clase auxiliar para el desplegable de andadores
public class WalkwayItem
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
}

public partial class RegisterViewModel : ObservableObject
{
    private readonly IRegistrationService _registrationService;
    private readonly IAlertService _alertService;
    private readonly INavigationService _navigationService;

    [ObservableProperty] public partial string FirstName { get; set; } = string.Empty;
    [ObservableProperty] public partial string LastName { get; set; } = string.Empty;
    [ObservableProperty] public partial string Email { get; set; } = string.Empty;
    [ObservableProperty] public partial string Password { get; set; } = string.Empty;
    [ObservableProperty] public partial WalkwayItem? SelectedWalkway { get; set; }
    [ObservableProperty] public partial bool IsBusy { get; set; }

    // Colección para alimentar el Picker de XAML. La organización a la que se une el vecino
    // (TenantConfig.DefaultOrganizationId) nunca se muestra ni se pide: es fija por despliegue.
    public ObservableCollection<WalkwayItem> Walkways { get; } = [];

    public ICommand LoadWalkwaysCommand { get; }

    public RegisterViewModel(IRegistrationService registrationService, IAlertService alertService, INavigationService navigationService)
    {
        _registrationService = registrationService;
        _alertService = alertService;
        _navigationService = navigationService;

        LoadWalkwaysCommand = new Command(async () => await LoadWalkwaysAsync());
        LoadWalkwaysCommand.Execute(null);
    }

    internal async Task LoadWalkwaysAsync()
    {
        try
        {
            var walkwaysFromApi = await _registrationService.GetPublicWalkwaysAsync(TenantConfig.DefaultOrganizationId);

            Walkways.Clear();

            if (walkwaysFromApi != null)
            {
                foreach (var walkway in walkwaysFromApi)
                {
                    Walkways.Add(new WalkwayItem { Id = walkway.Id, Code = walkway.Code });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Loading Walkways]: {ex.Message}");
        }
    }

    [RelayCommand]
    internal async Task RegisterAsync()
    {
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) ||
            string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgMissingRegisterData);
            return;
        }

        IsBusy = true;
        try
        {
            var result = await _registrationService.RegisterAsync(new CreateUserRequest
            {
                FirstName = FirstName.Trim(),
                LastName = LastName.Trim(),
                Email = Email.Trim(),
                Password = Password,
                OrganizationId = TenantConfig.DefaultOrganizationId,
                RoleId = TenantConfig.DefaultVecinoRoleId,
                WalkwayId = SelectedWalkway?.Id
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
