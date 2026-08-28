using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.Mobile.Infrastructure;
using IrrigationMaster.UI.Maui.Features.Level1_Core.Login;
using IrrigationMaster.UI.Maui.Features.Level1_Core.Register;
using IrrigationMaster.UI.Maui.Features.Level3_Functional.IrrigationPrograms;
using IrrigationMaster.UI.Maui.Features.Level3_Functional.Users;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.AdminConsole;
using IrrigationMaster.UI.Maui.Features.Level4_Operational.ApproveTurns;
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
        // El motor de red: ApiService se resuelve por todos sus contratos (Auth, Structure...).
        // Dos HttpClient independientes -- uno autenticado (acumula el Bearer de la sesión activa
        // vía AttachAuthHeadersAsync) y uno exclusivamente anónimo (RegisterAsync; nunca recibe
        // esa cabecera) -- para que una pantalla pensada para ser anónima no pueda arrastrar en
        // silencio el token de otra sesión ya abierta en el mismo cliente compartido. Ver
        // ApiService.ClearAuthHeader/_anonymousHttpClient.
        builder.Services.AddSingleton(sp => new ApiService(
            httpClient: new HttpClient
            {
                BaseAddress = new Uri(ApiConfig.BaseUrl),
                Timeout = TimeSpan.FromSeconds(15) // Evita que la app se quede colgada infinitamente en el campo
            },
            anonymousHttpClient: new HttpClient
            {
                BaseAddress = new Uri(ApiConfig.BaseUrl),
                Timeout = TimeSpan.FromSeconds(15)
            },
            tokenStorage: sp.GetRequiredService<ITokenStorage>()));
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
        builder.Services.AddTransient<IrrigationProgramsPage>();
        builder.Services.AddTransient<IrrigationProgramsViewModel>();

        // ─── NIVEL 4: OPERACIONAL (Admin Console) ───
        builder.Services.AddTransient<AdminMenuPage>();
        builder.Services.AddTransient<SystemSettingsPage>();
        builder.Services.AddTransient<SystemSettingsViewModel>();
        builder.Services.AddTransient<IrrigationStatusPage>();
        builder.Services.AddTransient<IrrigationStatusViewModel>();
        builder.Services.AddTransient<ApproveTurnsPage>();
        builder.Services.AddTransient<ApproveTurnsViewModel>();
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