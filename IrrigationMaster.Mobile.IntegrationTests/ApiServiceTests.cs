using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.IntegrationTests.TestDoubles;
using IrrigationMaster.Mobile.Infrastructure;
using System.Net;

namespace IrrigationMaster.Mobile.IntegrationTests;

public class ApiServiceTests
{
    private const string FakeBaseUrl = "https://fake-backend.test/api/v1/";

    private static ApiService CreateSut(FakeHttpMessageHandler handler, string? storedToken = "token-123")
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(FakeBaseUrl) };
        var tokenStorage = new FakeTokenStorage { StoredToken = storedToken };
        return new ApiService(httpClient, tokenStorage);
    }

    [Fact]
    public async Task CreateOrganizationAsync_PostsToCreateRoute_WithAuthHeaderAndNestedAddress()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Created);
        var sut = CreateSut(handler);
        var countryId = Guid.NewGuid();

        var (isSuccess, _) = await sut.CreateOrganizationAsync(new CreateOrganizationRequest
        {
            Name = "Regantes El Saso",
            TaxId = "G50123456",
            Address = new AddressRequest
            {
                MainAddress = "Camino Real s/n",
                City = "El Saso",
                StateOrProvince = "Huesca",
                PostalCode = "22300",
                CountryId = countryId
            }
        });

        Assert.True(isSuccess);
        Assert.EndsWith("organizations/Create", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization!.Scheme);
        Assert.Equal("token-123", handler.LastRequest.Headers.Authorization.Parameter);
        Assert.Contains("\"address\"", handler.LastRequestBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(countryId.ToString(), handler.LastRequestBody);
    }

    [Fact]
    public async Task CreateHydraulicSectorAsync_PostsToCreateRoute_WithAuthHeader_AndNoOrganizationId()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Created);
        var sut = CreateSut(handler);

        var (isSuccess, _) = await sut.CreateHydraulicSectorAsync(new CreateHydraulicSectorRequest
        {
            Name = "Sector Norte",
            AreaSize = 150.5m
        });

        Assert.True(isSuccess);
        Assert.EndsWith("hydraulicsectors/Create", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal("token-123", handler.LastRequest.Headers.Authorization!.Parameter);
        Assert.DoesNotContain("organizationId", handler.LastRequestBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateWalkwayAsync_PostsToCreateRoute_WithAuthHeader()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Created);
        var sut = CreateSut(handler);
        var sectorId = Guid.NewGuid();

        var (isSuccess, _) = await sut.CreateWalkwayAsync(new CreateWalkwayRequest
        {
            Code = "A-01",
            Length = 400m,
            HydraulicSectorId = sectorId
        });

        Assert.True(isSuccess);
        Assert.EndsWith("walkways/Create", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal("token-123", handler.LastRequest.Headers.Authorization!.Parameter);
        Assert.Contains(sectorId.ToString(), handler.LastRequestBody);
    }

    [Theory]
    [InlineData(HttpStatusCode.BadRequest)]
    [InlineData(HttpStatusCode.InternalServerError)]
    public async Task CreateOrganizationAsync_OnFailureStatus_ReturnsIsSuccessFalse(HttpStatusCode statusCode)
    {
        var handler = new FakeHttpMessageHandler(statusCode);
        var sut = CreateSut(handler);

        var (isSuccess, _) = await sut.CreateOrganizationAsync(new CreateOrganizationRequest());

        Assert.False(isSuccess);
    }

    [Fact]
    public async Task LoginAsync_WithFailedLoginAndValidationErrors_DeserializesErrorsArray()
    {
        const string responseJson = """
        {
            "isSuccess": false,
            "message": "Credenciales inválidas",
            "errors": [
                { "propertyMessage": "Email", "errorMessage": "El correo no tiene un formato válido" },
                { "propertyMessage": "Password", "errorMessage": "La contraseña es obligatoria" }
            ]
        }
        """;
        var handler = new FakeHttpMessageHandler(HttpStatusCode.BadRequest, responseJson);
        var sut = CreateSut(handler, storedToken: null);

        var result = await sut.LoginAsync("mal@correo", "");

        Assert.NotNull(result);
        Assert.False(result!.IsSuccess);
        Assert.Equal("Credenciales inválidas", result.Message);
        Assert.NotNull(result.Errors);
        Assert.Equal(2, result.Errors!.Count);
        Assert.Equal("Email", result.Errors[0].PropertyMessage);
        Assert.Equal("El correo no tiene un formato válido", result.Errors[0].ErrorMessage);
    }

    [Fact]
    public async Task LoginAsync_WithSuccessResponse_ReturnsData()
    {
        const string responseJson = """
        { "isSuccess": true, "message": "OK", "data": "un-jwt-cualquiera" }
        """;
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var sut = CreateSut(handler, storedToken: null);

        var result = await sut.LoginAsync("admin@elsaso.test", "clave-valida");

        Assert.NotNull(result);
        Assert.True(result!.IsSuccess);
        Assert.Equal("un-jwt-cualquiera", result.Data);
    }

    [Fact]
    public async Task LoginAsync_OnUnexpectedServerError_ReturnsServerErrorMessage()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError);
        var sut = CreateSut(handler, storedToken: null);

        var result = await sut.LoginAsync("admin@elsaso.test", "clave");

        Assert.NotNull(result);
        Assert.False(result!.IsSuccess);
        Assert.Contains("500", result.Message);
    }
}
