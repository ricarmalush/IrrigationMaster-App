using IrrigationMaster.Mobile.Application.Common.Dtos;
using IrrigationMaster.Mobile.Application.Constants;
using IrrigationMaster.Mobile.Application.Features.Models.Auth;
using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.Application.Features.Models.Structure.Country;
using IrrigationMaster.Mobile.Application.Interfaces;
using System.Net.Http.Json;

namespace IrrigationMaster.Mobile.Infrastructure;

public class ApiService : IAuthService, IStructureService
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
}