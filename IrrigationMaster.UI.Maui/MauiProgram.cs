using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.Mobile.Infrastructure;
using IrrigationMaster.UI.Maui.Features.Level1_Core.Login;
using IrrigationMaster.UI.Maui.Features.Level1_Core.Register;
using IrrigationMaster.UI.Maui.Features.Level3_Functional.Users;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.AdminConsole;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.IrrigationStatus;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.CommunityBroadcast;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.Notifications;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.ReportIncident;
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
        // Fuente única de verdad de la sesión activa (parsea el JWT una sola vez, en Infrastructure)
        builder.Services.AddSingleton<ICurrentSession, CurrentSession>();
        // El motor de red: una única instancia, resuelta por sus dos contratos (Auth y Structure)
        builder.Services.AddSingleton(sp => new HttpClient
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl),
            Timeout = TimeSpan.FromSeconds(15) // Evita que la app se quede colgada infinitamente en el campo
        });
        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<IAuthService>(sp => sp.GetRequiredService<ApiService>());
        builder.Services.AddSingleton<IStructureService>(sp => sp.GetRequiredService<ApiService>());
        builder.Services.AddSingleton<IRegistrationService>(sp => sp.GetRequiredService<ApiService>());
        builder.Services.AddSingleton<IUserManagementService>(sp => sp.GetRequiredService<ApiService>());
        builder.Services.AddSingleton<IIrrigationService>(sp => sp.GetRequiredService<ApiService>());
        builder.Services.AddSingleton<INotificationService>(sp => sp.GetRequiredService<ApiService>());
        builder.Services.AddSingleton<IAlertService, ShellAlertService>();
        builder.Services.AddSingleton<INavigationService, ShellNavigationService>();

        // ─── NIVEL 1: CORE (Autenticación) ───
        builder.Services.AddTransient<MainPage>(); // Asumo que esta es tu página de Login principal
        builder.Services.AddTransient<LoginViewModel>();
        builder.Services.AddTransient<RegisterPage>();
        builder.Services.AddTransient<RegisterViewModel>();

        // ─── NIVEL 3: FUNCIONAL (Usuarios y Roles) ───
        builder.Services.AddTransient<UserManagementPage>();
        builder.Services.AddTransient<UserManagementViewModel>();

        // ─── NIVEL 4: OPERACIONAL (Admin Console) ───
        builder.Services.AddTransient<AdminMenuPage>();
        builder.Services.AddTransient<SystemSettingsPage>();
        builder.Services.AddTransient<SystemSettingsViewModel>();
        builder.Services.AddTransient<IrrigationStatusPage>();
        builder.Services.AddTransient<IrrigationStatusViewModel>();
        builder.Services.AddTransient<NotificationsPage>();
        builder.Services.AddTransient<NotificationsViewModel>();
        builder.Services.AddTransient<ReportIncidentPage>();
        builder.Services.AddTransient<ReportIncidentViewModel>();
        builder.Services.AddTransient<CommunityBroadcastPage>();
        builder.Services.AddTransient<CommunityBroadcastViewModel>();

#if DEBUG
        builder.Logging.AddDebug();
#endif

        return builder.Build();
    }
}