using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.UI.Maui.Common;
using IrrigationMaster.UI.Maui.Features.Level1_Core.Register;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.AdminConsole;
using System.Diagnostics;

namespace IrrigationMaster.UI.Maui.Services;

public class ShellNavigationService : INavigationService
{
    public async Task GoToAsync(string route)
    {
        try
        {
            await Shell.Current.GoToAsync(route);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[Shell Navigation Error]: {ex.Message}");
            await FallbackPushAsync(route);
        }
    }

    // Plan de respaldo si el enrutamiento de Shell falla (navegación jerárquica directa),
    // igual comportamiento que existía antes en LoginViewModel.
    private static async Task FallbackPushAsync(string route)
    {
        var currentPage = Shell.Current?.CurrentPage;
        if (currentPage == null)
            return;

        switch (route)
        {
            case "//AdminMenuPage":
                await currentPage.Navigation.PushAsync(new AdminMenuPage());
                break;

            case "RegisterPage":
                var services = Shell.Current!.Handler?.MauiContext?.Services;
                var registerViewModel = services?.GetService<RegisterViewModel>();

                if (registerViewModel != null)
                    await currentPage.Navigation.PushAsync(new RegisterPage(registerViewModel));
                else
                    await currentPage.DisplayAlert(AppStrings.SystemErrorTitle, AppStrings.NavigationFallbackError, "OK");
                break;
        }
    }
}
