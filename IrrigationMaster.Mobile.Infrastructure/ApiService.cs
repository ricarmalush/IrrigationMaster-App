using IrrigationMaster.Mobile.Application.Common.Dtos;
using IrrigationMaster.Mobile.Application.Constants;
using IrrigationMaster.Mobile.Application.Features.Models.Auth;
using IrrigationMaster.Mobile.Application.Features.Models.Irrigation;
using IrrigationMaster.Mobile.Application.Features.Models.Notifications;
using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.Application.Features.Models.Structure.Country;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;
using System.Net.Http.Json;

namespace IrrigationMaster.Mobile.Infrastructure;

public class ApiService : IAuthService, IStructureService, IRegistrationService, IUserManagementService, IIrrigationService, INotificationService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStorage _tokenStorage;

    // El HttpClient se recibe ya configurado (BaseAddress/Timeout) para poder
    // sustituirlo por uno con un HttpMessageHandler falso en los tests.
    public ApiService(HttpClient httpClient, ITokenStorage tokenStorage)
    {
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
    }

    // ─── 1. AUTENTICACIÓN ───

    public async Task<LoginResponse?> LoginAsync(string email, string password)
    {
        try
        {
            var requestData = new LoginRequest
            {
                Email = email,
                Password = password
            };

            var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Login, requestData);

            if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
            {
                return await response.Content.ReadFromJsonAsync<LoginResponse>();
            }

            return new LoginResponse
            {
                IsSuccess = false,
                Message = $"{ServiceMessages.ServerErrorCode} {(int)response.StatusCode})"
            };
        }
        catch (HttpRequestException)
        {
            return new LoginResponse { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        }
        catch (Exception)
        {
            return new LoginResponse { IsSuccess = false, Message = ServiceMessages.UnexpectedError };
        }
    }

    public async Task<UserActionResult> ChangePasswordAsync(string currentPassword, string newPassword, string confirmNewPassword)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PutAsJsonAsync(ApiEndpoints.UsersChangePassword, new ChangePasswordRequest
            {
                CurrentPassword = currentPassword,
                NewPassword = newPassword,
                ConfirmNewPassword = confirmNewPassword
            });
            return await ReadUserActionResultAsync(response);
        }
        catch (HttpRequestException)
        {
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - ChangePassword]: {ex.Message}");
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.ApiConnectionError };
        }
    }

    // ─── 2. CONFIGURACIÓN DE ESTRUCTURA (DOMINIO FÍSICO) ───

    public async Task<StructureOperationResult> CreateOrganizationAsync(CreateOrganizationRequest request)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Organizations, request);
            return await ReadStructureResultAsync(response);
        }
        catch (HttpRequestException)
        {
            return new StructureOperationResult { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - Org]: {ex.Message}");
            return new StructureOperationResult { IsSuccess = false, Message = ServiceMessages.ApiConnectionError };
        }
    }

    public async Task<StructureOperationResult> CreateHydraulicSectorAsync(CreateHydraulicSectorRequest request)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.HydraulicSectors, request);
            return await ReadStructureResultAsync(response);
        }
        catch (HttpRequestException)
        {
            return new StructureOperationResult { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - Sector]: {ex.Message}");
            return new StructureOperationResult { IsSuccess = false, Message = ServiceMessages.ApiConnectionError };
        }
    }

    public async Task<StructureOperationResult> CreateWalkwayAsync(CreateWalkwayRequest request)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Walkways, request);
            return await ReadStructureResultAsync(response);
        }
        catch (HttpRequestException)
        {
            return new StructureOperationResult { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - Walkway]: {ex.Message}");
            return new StructureOperationResult { IsSuccess = false, Message = ServiceMessages.ApiConnectionError };
        }
    }

    // El backend también manda un body parseable en 400 (errores de validación), no solo en 2xx.
    private static async Task<StructureOperationResult> ReadStructureResultAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.BadRequest)
        {
            var result = await response.Content.ReadFromJsonAsync<StructureOperationResult>();
            return result ?? new StructureOperationResult { IsSuccess = false, Message = ServiceMessages.UnexpectedError };
        }

        return new StructureOperationResult
        {
            IsSuccess = false,
            Message = $"{ServiceMessages.ServerErrorCode} {(int)response.StatusCode})"
        };
    }

    private async Task AttachAuthHeadersAsync()
    {
        var token = await _tokenStorage.GetTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
    }

    public async Task<List<CountryDto>?> GetCountriesAsync()
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.GetAsync($"{ApiEndpoints.Countries}?PageNumber=1&PageSize=100");

            if (response.IsSuccessStatusCode)
            {
                var paged = await response.Content.ReadFromJsonAsync<PagedResponse<List<CountryDto>>>();
                return paged?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - Countries]: {ex.Message}");
            return null;
        }
    }

    public async Task<List<HydraulicSectorDto>?> GetHydraulicSectorsAsync()
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.GetAsync($"{ApiEndpoints.HydraulicSectorsPagination}?PageNumber=1&PageSize=100");

            if (response.IsSuccessStatusCode)
            {
                var paged = await response.Content.ReadFromJsonAsync<PagedResponse<List<HydraulicSectorDto>>>();
                return paged?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - HydraulicSectors]: {ex.Message}");
            return null;
        }
    }

    public async Task<OrganizationDto?> GetOrganizationAsync(Guid organizationId)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.GetAsync($"{ApiEndpoints.OrganizationsGet}/{organizationId}");

            if (response.IsSuccessStatusCode)
            {
                var wrapped = await response.Content.ReadFromJsonAsync<OrganizationDetailsResponse>();
                return wrapped?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - Organization]: {ex.Message}");
            return null;
        }
    }

    public async Task<List<OrganizationDto>?> GetOrganizationsAsync()
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.GetAsync($"{ApiEndpoints.OrganizationsPagination}?PageNumber=1&PageSize=100");

            if (response.IsSuccessStatusCode)
            {
                var paged = await response.Content.ReadFromJsonAsync<PagedResponse<List<OrganizationDto>>>();
                return paged?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - Organizations]: {ex.Message}");
            return null;
        }
    }

    public async Task<WalkwayDetailDto?> GetWalkwayAsync(Guid walkwayId)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.GetAsync($"{ApiEndpoints.WalkwaysGet}/{walkwayId}");

            if (response.IsSuccessStatusCode)
            {
                var wrapped = await response.Content.ReadFromJsonAsync<WalkwayDetailResponse>();
                return wrapped?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - Walkway]: {ex.Message}");
            return null;
        }
    }

    // ─── 3. AUTO-REGISTRO (ANÓNIMO) ───

    public async Task<StructureOperationResult> RegisterAsync(CreateUserRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Users, request);
            return await ReadStructureResultAsync(response);
        }
        catch (HttpRequestException)
        {
            return new StructureOperationResult { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - Register]: {ex.Message}");
            return new StructureOperationResult { IsSuccess = false, Message = ServiceMessages.ApiConnectionError };
        }
    }

    public async Task<List<PublicWalkwayDto>?> GetPublicWalkwaysAsync(Guid organizationId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"{ApiEndpoints.WalkwaysPublic}?organizationId={organizationId}");

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<PublicWalkwaysResponse>();
                return result?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - PublicWalkways]: {ex.Message}");
            return null;
        }
    }

    // ─── 4. GESTIÓN DE USUARIOS/ROLES (AUTENTICADO) ───

    public async Task<List<AppUserDto>?> GetUsersAsync(bool? isActive, Guid? organizationId = null)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var url = $"{ApiEndpoints.UsersPagination}?PageNumber=1&PageSize=100";
            if (isActive.HasValue)
                url += $"&IsActive={isActive.Value}";
            if (organizationId.HasValue)
                url += $"&OrganizationId={organizationId.Value}";

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var paged = await response.Content.ReadFromJsonAsync<PagedResponse<List<AppUserDto>>>();
                return paged?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - Users]: {ex.Message}");
            return null;
        }
    }

    public async Task<AppUserDto?> GetUserByIdAsync(Guid userId)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.GetAsync($"{ApiEndpoints.UsersGet}/{userId}");

            if (response.IsSuccessStatusCode)
            {
                var wrapped = await response.Content.ReadFromJsonAsync<UserDetailResponse>();
                return wrapped?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - GetUserById]: {ex.Message}");
            return null;
        }
    }

    public async Task<List<RoleDto>?> GetRolesAsync()
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.GetAsync($"{ApiEndpoints.RolesPagination}?PageNumber=1&PageSize=100");

            if (response.IsSuccessStatusCode)
            {
                var paged = await response.Content.ReadFromJsonAsync<PagedResponse<List<RoleDto>>>();
                return paged?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - Roles]: {ex.Message}");
            return null;
        }
    }

    public async Task<List<WalkwayDto>?> GetWalkwaysAsync()
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.GetAsync($"{ApiEndpoints.WalkwaysPagination}?PageNumber=1&PageSize=100");

            if (response.IsSuccessStatusCode)
            {
                var paged = await response.Content.ReadFromJsonAsync<PagedResponse<List<WalkwayDto>>>();
                return paged?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - Walkways]: {ex.Message}");
            return null;
        }
    }

    public async Task<UserActionResult> ActivateUserAsync(Guid userId)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PutAsync($"{ApiEndpoints.UsersActivate}/{userId}", null);
            return await ReadUserActionResultAsync(response);
        }
        catch (HttpRequestException)
        {
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - ActivateUser]: {ex.Message}");
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.ApiConnectionError };
        }
    }

    public async Task<UserActionResult> AssignWalkwayAsync(Guid userId, Guid? walkwayId)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoints.UsersAssignWalkway}/{userId}", new AssignWalkwayRequest { WalkwayId = walkwayId });
            return await ReadUserActionResultAsync(response);
        }
        catch (HttpRequestException)
        {
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - AssignWalkway]: {ex.Message}");
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.ApiConnectionError };
        }
    }

    public async Task<UserActionResult> ChangeRoleAsync(Guid userId, Guid roleId)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoints.UsersChangeRole}/{userId}", new ChangeRoleRequest { RoleId = roleId });
            return await ReadUserActionResultAsync(response);
        }
        catch (HttpRequestException)
        {
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - ChangeRole]: {ex.Message}");
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.ApiConnectionError };
        }
    }

    public async Task<UserActionResult> ResetPasswordAsync(Guid userId, string newPassword)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PutAsJsonAsync($"{ApiEndpoints.UsersResetPassword}/{userId}", new ResetPasswordRequest { NewPassword = newPassword });
            return await ReadUserActionResultAsync(response);
        }
        catch (HttpRequestException)
        {
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - ResetPassword]: {ex.Message}");
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.ApiConnectionError };
        }
    }

    // ─── 5. ESTADO DE RIEGO ───

    public async Task<List<WalkwayIrrigationStatusDto>?> GetIrrigationStatusAsync()
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.GetAsync(ApiEndpoints.IrrigationTurnsStatus);

            if (response.IsSuccessStatusCode)
            {
                var wrapped = await response.Content.ReadFromJsonAsync<IrrigationStatusResponse>();
                return wrapped?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - IrrigationStatus]: {ex.Message}");
            return null;
        }
    }

    public async Task<UserActionResult> StartTurnAsync(Guid turnId)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PatchAsync($"{ApiEndpoints.IrrigationTurns}/{turnId}/start", null);
            return await ReadUserActionResultAsync(response);
        }
        catch (HttpRequestException)
        {
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - StartTurn]: {ex.Message}");
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.ApiConnectionError };
        }
    }

    public async Task<UserActionResult> CompleteTurnAsync(Guid turnId)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PatchAsync($"{ApiEndpoints.IrrigationTurns}/{turnId}/complete", null);
            return await ReadUserActionResultAsync(response);
        }
        catch (HttpRequestException)
        {
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - CompleteTurn]: {ex.Message}");
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.ApiConnectionError };
        }
    }

    public async Task<bool> IsIrrigationDayAsync(Guid hydraulicSectorId)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.GetAsync($"{ApiEndpoints.IrrigationProgramsIsIrrigationDay}?HydraulicSectorId={hydraulicSectorId}");

            if (response.IsSuccessStatusCode)
            {
                var wrapped = await response.Content.ReadFromJsonAsync<IsIrrigationDayResponse>();
                // Fallback true ("sin actividad todavía") en vez de false ("no hay riego programado
                // hoy"): ante un fallo de red o de parseo, es preferible no afirmar algo que no
                // sabemos que sea cierto.
                return wrapped?.Data ?? true;
            }

            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - IsIrrigationDay]: {ex.Message}");
            return true;
        }
    }

    public async Task<List<IrrigationProgramDto>?> GetIrrigationProgramsAsync()
    {
        try
        {
            await AttachAuthHeadersAsync();

            // A diferencia del resto de listados, este endpoint exige OrganizationId explícito en
            // la query (no lo resuelve por sí solo vía ICurrentUser) -- lo tomamos de la sesión
            // guardada, la misma fuente que usa CurrentSession.GetOrganizationIdAsync.
            var organizationId = await _tokenStorage.GetOrganizationIdAsync();
            if (string.IsNullOrWhiteSpace(organizationId))
                return null;

            var response = await _httpClient.GetAsync($"{ApiEndpoints.IrrigationProgramsPagination}?OrganizationId={organizationId}&PageNumber=1&PageSize=100");

            if (response.IsSuccessStatusCode)
            {
                var paged = await response.Content.ReadFromJsonAsync<PagedResponse<List<IrrigationProgramDto>>>();
                return paged?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - IrrigationPrograms]: {ex.Message}");
            return null;
        }
    }

    // ─── 6. NOTIFICACIONES ───

    public async Task<List<NotificationDto>?> GetMyNotificationsAsync()
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.GetAsync($"{ApiEndpoints.NotificationsMine}?PageNumber=1&PageSize=100");

            if (response.IsSuccessStatusCode)
            {
                var paged = await response.Content.ReadFromJsonAsync<PagedResponse<List<NotificationDto>>>();
                return paged?.Data;
            }

            return null;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - Notifications]: {ex.Message}");
            return null;
        }
    }

    public async Task<UserActionResult> MarkAsReadAsync(Guid notificationId)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PutAsync($"{ApiEndpoints.NotificationsMarkAsRead}/{notificationId}", null);
            return await ReadUserActionResultAsync(response);
        }
        catch (HttpRequestException)
        {
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - MarkAsRead]: {ex.Message}");
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.ApiConnectionError };
        }
    }

    public async Task<UserActionResult> MarkAllAsReadAsync()
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PutAsync(ApiEndpoints.NotificationsMarkAllAsRead, null);
            return await ReadUserActionResultAsync(response);
        }
        catch (HttpRequestException)
        {
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - MarkAllAsRead]: {ex.Message}");
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.ApiConnectionError };
        }
    }

    public async Task<UserActionResult> ReportIncidentAsync(string message)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.NotificationsReportIncident, new ReportIncidentRequest { Message = message });
            return await ReadUserActionResultAsync(response);
        }
        catch (HttpRequestException)
        {
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - ReportIncident]: {ex.Message}");
            return new UserActionResult { IsSuccess = false, Message = ServiceMessages.ApiConnectionError };
        }
    }

    // Título fijo: la pantalla "Avisar a mi comunidad" solo pide el mensaje, no un título aparte,
    // pero el backend exige Title no vacío en SendNotificationCommand.
    private const string BroadcastTitle = "Aviso de tu comunidad";

    public async Task<SendNotificationResult> SendNotificationAsync(string audience, string message, Guid? targetWalkwayId)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.NotificationsSend, new SendNotificationRequest
            {
                Audience = audience,
                Title = BroadcastTitle,
                Message = message,
                Type = "Info",
                TargetWalkwayId = targetWalkwayId
            });

            if (response.IsSuccessStatusCode ||
                response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
                response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var result = await response.Content.ReadFromJsonAsync<SendNotificationResult>();
                return result ?? new SendNotificationResult { IsSuccess = false, Message = ServiceMessages.UnexpectedError };
            }

            return new SendNotificationResult
            {
                IsSuccess = false,
                Message = $"{ServiceMessages.ServerErrorCode} {(int)response.StatusCode})"
            };
        }
        catch (HttpRequestException)
        {
            return new SendNotificationResult { IsSuccess = false, Message = ServiceMessages.NetworkConnectionError };
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - SendNotification]: {ex.Message}");
            return new SendNotificationResult { IsSuccess = false, Message = ServiceMessages.ApiConnectionError };
        }
    }

    // El backend también manda un body parseable en 400 (p. ej. permiso insuficiente) y en 403.
    private static async Task<UserActionResult> ReadUserActionResultAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode ||
            response.StatusCode == System.Net.HttpStatusCode.BadRequest ||
            response.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            var result = await response.Content.ReadFromJsonAsync<UserActionResult>();
            return result ?? new UserActionResult { IsSuccess = false, Message = ServiceMessages.UnexpectedError };
        }

        return new UserActionResult
        {
            IsSuccess = false,
            Message = $"{ServiceMessages.ServerErrorCode} {(int)response.StatusCode})"
        };
    }
}