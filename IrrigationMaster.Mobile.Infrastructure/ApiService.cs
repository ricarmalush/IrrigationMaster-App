using IrrigationMaster.Mobile.Core.Constants;
using IrrigationMaster.Mobile.Core.Features.Models.Auth;
using IrrigationMaster.Mobile.Core.Features.Models.Structure;
using IrrigationMaster.Mobile.Core.Features.Models.Structure.Country;
using IrrigationMaster.Mobile.Core.Interfaces;
using System.Net.Http.Json;

namespace IrrigationMaster.Mobile.Infrastructure;

public class ApiService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly ITokenStorage _tokenStorage;

    public ApiService(ITokenStorage tokenStorage)
    {
        _tokenStorage = tokenStorage;
        _httpClient = new HttpClient
        {
            BaseAddress = new Uri(ApiConfig.BaseUrl),
            Timeout = TimeSpan.FromSeconds(15) // Evita que la app se quede colgada infinitamente en el campo
        };
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

    public async Task<(bool IsSuccess, string Message)> CreateOrganizationAsync(CreateOrganizationRequest request)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Organizations, request);

            if (response.IsSuccessStatusCode)
                return (true, ServiceMessages.OrgCreatedSuccess);

            return (false, ServiceMessages.OrgCreatedError);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - Org]: {ex.Message}");
            return (false, ServiceMessages.ApiConnectionError);
        }
    }

    public async Task<(bool IsSuccess, string Message)> CreateHydraulicSectorAsync(CreateHydraulicSectorRequest request)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.HydraulicSectors, request);

            if (response.IsSuccessStatusCode)
                return (true, ServiceMessages.SectorCreatedSuccess);

            return (false, ServiceMessages.SectorCreatedError);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - Sector]: {ex.Message}");
            return (false, ServiceMessages.ApiConnectionError);
        }
    }

    public async Task<(bool IsSuccess, string Message)> CreateWalkwayAsync(CreateWalkwayRequest request)
    {
        try
        {
            await AttachAuthHeadersAsync();

            var response = await _httpClient.PostAsJsonAsync(ApiEndpoints.Walkways, request);

            if (response.IsSuccessStatusCode)
                return (true, ServiceMessages.WalkwayCreatedSuccess);

            return (false, ServiceMessages.WalkwayCreatedError);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[API Error - Walkway]: {ex.Message}");
            return (false, ServiceMessages.ApiConnectionError);
        }
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
}