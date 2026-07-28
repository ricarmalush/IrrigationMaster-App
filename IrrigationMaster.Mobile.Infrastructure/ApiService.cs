using IrrigationMaster.Mobile.Application.Common.Dtos;
using IrrigationMaster.Mobile.Application.Constants;
using IrrigationMaster.Mobile.Application.Features.Models.Auth;
using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.Application.Features.Models.Structure.Country;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;
using System.Net.Http.Json;

namespace IrrigationMaster.Mobile.Infrastructure;

public class ApiService : IAuthService, IStructureService, IRegistrationService, IUserManagementService
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

            var response = await _httpClient.GetAsync($"{ApiEndpoints.Countries}?PageNumber=1&PageSize=200");

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

            var response = await _httpClient.GetAsync($"{ApiEndpoints.HydraulicSectorsPagination}?PageNumber=1&PageSize=200");

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

    public async Task<List<AppUserDto>?> GetUsersAsync(bool? isActive)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var url = $"{ApiEndpoints.UsersPagination}?PageNumber=1&PageSize=200";
            if (isActive.HasValue)
                url += $"&IsActive={isActive.Value}";

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

    public async Task<List<RoleDto>?> GetRolesAsync()
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.GetAsync($"{ApiEndpoints.RolesPagination}?PageNumber=1&PageSize=200");

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

            var response = await _httpClient.GetAsync($"{ApiEndpoints.WalkwaysPagination}?PageNumber=1&PageSize=200");

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