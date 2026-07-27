using IrrigationMaster.Mobile.Core.Interfaces;
using IrrigationMaster.Mobile.Infrastructure;
using IrrigationMaster.UI.Maui.Features.Level1_Core.Login;
using IrrigationMaster.UI.Maui.Features.Level1_Core.Register;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.AdminConsole;
using IrrigationMaster.UI.Maui.Services;
using Microsoft.Extensions.Logging;

namespace IrrigationMaster.UI.Maui;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

        // ─── INFRAESTRUCTURA ───
        // El almacenamiento seguro de sesión (token + organización + rol)
        builder.Services.AddSingleton<ITokenStorage, SecureTokenStorage>();
        // El motor de red, expuesto a través de su contrato IAuthService
        builder.Services.AddSingleton<IAuthService, ApiService>();

        // ─── NIVEL 1: CORE (Autenticación) ───
        builder.Services.AddTransient<MainPage>(); // Asumo que esta es tu página de Login principal
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<RegisterViewModel>();

        // ─── NIVEL 4: OPERACIONAL (Admin Console) ───
        builder.Services.AddTransient<AdminMenuPage>();
        builder.Services.AddTransient<SystemSettingsPage>();
        builder.Services.AddTransient<SystemSettingsViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}