using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.IntegrationTests.TestDoubles;
using IrrigationMaster.Mobile.Infrastructure;
using System.Net;

namespace IrrigationMaster.Mobile.IntegrationTests;

public class ApiServiceTests
{
    private const string FakeBaseUrl = "https://fake-backend.test/api/v1/";
    private const string CreatedResponseJson = """{ "isSuccess": true, "message": "Operación completada exitosamente.", "data": "3fa85f64-5717-4562-b3fc-2c963f66afa6" }""";

    private static ApiService CreateSut(FakeHttpMessageHandler handler, string? storedToken = "token-123")
    {
        var httpClient = new HttpClient(handler) { BaseAddress = new Uri(FakeBaseUrl) };
        var tokenStorage = new FakeTokenStorage { StoredToken = storedToken };
        return new ApiService(httpClient, tokenStorage);
    }

    [Fact]
    public async Task CreateOrganizationAsync_PostsToCreateRoute_WithAuthHeaderAndNestedAddress()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Created, CreatedResponseJson);
        var sut = CreateSut(handler);
        var countryId = Guid.NewGuid();

        var result = await sut.CreateOrganizationAsync(new CreateOrganizationRequest
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

        Assert.True(result.IsSuccess);
        Assert.EndsWith("organizations/Create", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization!.Scheme);
        Assert.Equal("token-123", handler.LastRequest.Headers.Authorization.Parameter);
        Assert.Contains("\"address\"", handler.LastRequestBody, StringComparison.OrdinalIgnoreCase);
        Assert.Contains(countryId.ToString(), handler.LastRequestBody);
    }

    [Fact]
    public async Task CreateHydraulicSectorAsync_PostsToCreateRoute_WithAuthHeader_AndNoOrganizationId()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Created, CreatedResponseJson);
        var sut = CreateSut(handler);

        var result = await sut.CreateHydraulicSectorAsync(new CreateHydraulicSectorRequest
        {
            Name = "Sector Norte",
            AreaSize = 150.5m
        });

        Assert.True(result.IsSuccess);
        Assert.EndsWith("hydraulicsectors/Create", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal("token-123", handler.LastRequest.Headers.Authorization!.Parameter);
        Assert.DoesNotContain("organizationId", handler.LastRequestBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task CreateWalkwayAsync_PostsToCreateRoute_WithAuthHeader()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Created, CreatedResponseJson);
        var sut = CreateSut(handler);
        var sectorId = Guid.NewGuid();

        var result = await sut.CreateWalkwayAsync(new CreateWalkwayRequest
        {
            Code = "A-01",
            Length = 400m,
            HydraulicSectorId = sectorId
        });

        Assert.True(result.IsSuccess);
        Assert.EndsWith("walkways/Create", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal("token-123", handler.LastRequest.Headers.Authorization!.Parameter);
        Assert.Contains(sectorId.ToString(), handler.LastRequestBody);
    }

    [Fact]
    public async Task CreateOrganizationAsync_OnValidationFailure_ReturnsBackendErrors()
    {
        const string responseJson = """
        {
            "isSuccess": false,
            "message": "Datos inválidos",
            "errors": [
                { "propertyMessage": "TaxId", "errorMessage": "El NIF ya está registrado" }
            ]
        }
        """;
        var handler = new FakeHttpMessageHandler(HttpStatusCode.BadRequest, responseJson);
        var sut = CreateSut(handler);

        var result = await sut.CreateOrganizationAsync(new CreateOrganizationRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal("Datos inválidos", result.Message);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors!);
        Assert.Equal("TaxId", result.Errors![0].PropertyMessage);
    }

    [Fact]
    public async Task CreateOrganizationAsync_OnServerError_ReturnsIsSuccessFalse()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError);
        var sut = CreateSut(handler);

        var result = await sut.CreateOrganizationAsync(new CreateOrganizationRequest());

        Assert.False(result.IsSuccess);
        Assert.Contains("500", result.Message);
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

    // ─── AUTO-REGISTRO (ANÓNIMO) ───

    [Fact]
    public async Task RegisterAsync_PostsToUsersCreateRoute_WithoutAuthHeader_AndTenantValues()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.Created, CreatedResponseJson);
        var sut = CreateSut(handler, storedToken: null);
        const string invitationCode = "7XQZ9MKT";
        var roleId = Guid.NewGuid();

        var result = await sut.RegisterAsync(new CreateUserRequest
        {
            FirstName = "Ana",
            LastName = "García",
            Email = "ana@correo.test",
            Password = "clave12345",
            InvitationCode = invitationCode,
            RoleId = roleId
        });

        Assert.True(result.IsSuccess);
        Assert.EndsWith("Users/Create", handler.LastRequest!.RequestUri!.ToString());
        Assert.Null(handler.LastRequest.Headers.Authorization);
        Assert.Contains(invitationCode, handler.LastRequestBody);
        Assert.Contains(roleId.ToString(), handler.LastRequestBody);
        Assert.DoesNotContain("organizationId", handler.LastRequestBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task RegisterAsync_OnValidationFailure_ReturnsBackendErrors()
    {
        const string responseJson = """
        {
            "isSuccess": false,
            "message": "Datos inválidos",
            "errors": [
                { "propertyMessage": "Email", "errorMessage": "Ya existe una cuenta con ese correo" }
            ]
        }
        """;
        var handler = new FakeHttpMessageHandler(HttpStatusCode.BadRequest, responseJson);
        var sut = CreateSut(handler, storedToken: null);

        var result = await sut.RegisterAsync(new CreateUserRequest());

        Assert.False(result.IsSuccess);
        Assert.Equal("Datos inválidos", result.Message);
        Assert.NotNull(result.Errors);
        Assert.Single(result.Errors!);
        Assert.Equal("Email", result.Errors![0].PropertyMessage);
    }

    [Fact]
    public async Task RegisterAsync_OnServerError_ReturnsIsSuccessFalse()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError);
        var sut = CreateSut(handler, storedToken: null);

        var result = await sut.RegisterAsync(new CreateUserRequest());

        Assert.False(result.IsSuccess);
        Assert.Contains("500", result.Message);
    }

    [Fact]
    public async Task GetPublicWalkwaysAsync_ReturnsWalkwaysForOrganization()
    {
        var organizationId = Guid.NewGuid();
        var walkwayId = Guid.NewGuid();
        var responseJson = $$"""
        {
            "isSuccess": true,
            "message": "OK",
            "data": [ { "id": "{{walkwayId}}", "code": "A-01" } ]
        }
        """;
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var sut = CreateSut(handler, storedToken: null);

        var result = await sut.GetPublicWalkwaysAsync(organizationId);

        Assert.NotNull(result);
        var walkway = Assert.Single(result!);
        Assert.Equal(walkwayId, walkway.Id);
        Assert.Equal("A-01", walkway.Code);
        Assert.Contains($"organizationId={organizationId}", handler.LastRequest!.RequestUri!.ToString());
        Assert.Null(handler.LastRequest.Headers.Authorization);
    }

    [Fact]
    public async Task GetPublicWalkwaysAsync_OnFailureStatus_ReturnsNull()
    {
        var handler = new FakeHttpMessageHandler(HttpStatusCode.InternalServerError);
        var sut = CreateSut(handler, storedToken: null);

        var result = await sut.GetPublicWalkwaysAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    // ─── GESTIÓN DE USUARIOS/ROLES (AUTENTICADO) ───

    [Fact]
    public async Task GetUsersAsync_WithIsActiveFalse_AppendsFilterToQueryString_WithAuthHeader()
    {
        const string responseJson = """{ "isSuccess": true, "message": "OK", "data": [], "totalCount": 0, "pageNumber": 1, "pageSize": 200 }""";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var sut = CreateSut(handler);

        var result = await sut.GetUsersAsync(false);

        Assert.NotNull(result);
        Assert.Contains("Users/pagination", handler.LastRequest!.RequestUri!.ToString());
        Assert.Contains("IsActive=False", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("token-123", handler.LastRequest.Headers.Authorization!.Parameter);
    }

    [Fact]
    public async Task GetUsersAsync_WithNullIsActive_DoesNotAppendFilter()
    {
        const string responseJson = """{ "isSuccess": true, "message": "OK", "data": [], "totalCount": 0, "pageNumber": 1, "pageSize": 200 }""";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var sut = CreateSut(handler);

        await sut.GetUsersAsync(null);

        Assert.DoesNotContain("IsActive", handler.LastRequest!.RequestUri!.ToString());
    }

    [Fact]
    public async Task GetRolesAsync_ReturnsRoles_WithAuthHeader()
    {
        var roleId = Guid.NewGuid();
        var responseJson = $$"""
        {
            "isSuccess": true,
            "message": "OK",
            "data": [ { "id": "{{roleId}}", "name": "Tesorero", "organizationId": "3fa85f64-5717-4562-b3fc-2c963f66afa6" } ],
            "totalCount": 1, "pageNumber": 1, "pageSize": 200
        }
        """;
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var sut = CreateSut(handler);

        var result = await sut.GetRolesAsync();

        Assert.NotNull(result);
        var role = Assert.Single(result!);
        Assert.Equal(roleId, role.Id);
        Assert.Equal("Tesorero", role.Name);
        Assert.Contains("Roles/pagination", handler.LastRequest!.RequestUri!.ToString());
        Assert.Equal("token-123", handler.LastRequest.Headers.Authorization!.Parameter);
    }

    [Fact]
    public async Task GetWalkwaysAsync_ReturnsWalkways_WithAuthHeader_NoOrganizationIdInQuery()
    {
        // A diferencia de GetPublicWalkwaysAsync (flujo anónimo de registro), este endpoint está
        // autenticado y el backend lo acota a la organización del llamador vía ICurrentUser --
        // no debe viajar ningún organizationId en la query string.
        var walkwayId = Guid.NewGuid();
        var responseJson = $$"""
        {
            "isSuccess": true,
            "message": "OK",
            "data": [ { "id": "{{walkwayId}}", "code": "A-01" } ],
            "totalCount": 1, "pageNumber": 1, "pageSize": 200
        }
        """;
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var sut = CreateSut(handler);

        var result = await sut.GetWalkwaysAsync();

        Assert.NotNull(result);
        var walkway = Assert.Single(result!);
        Assert.Equal(walkwayId, walkway.Id);
        Assert.Equal("A-01", walkway.Code);
        Assert.Contains("walkways/pagination", handler.LastRequest!.RequestUri!.ToString());
        Assert.DoesNotContain("organizationId", handler.LastRequest.RequestUri!.ToString(), StringComparison.OrdinalIgnoreCase);
        Assert.Equal("token-123", handler.LastRequest.Headers.Authorization!.Parameter);
    }

    [Fact]
    public async Task ActivateUserAsync_PutsToActivateRoute_WithAuthHeader()
    {
        const string responseJson = """{ "isSuccess": true, "message": "OK" }""";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var sut = CreateSut(handler);
        var userId = Guid.NewGuid();

        var result = await sut.ActivateUserAsync(userId);

        Assert.True(result.IsSuccess);
        Assert.Equal(HttpMethod.Put, handler.LastRequest!.Method);
        Assert.EndsWith($"Users/Activate/{userId}", handler.LastRequest.RequestUri!.ToString());
        Assert.Equal("token-123", handler.LastRequest.Headers.Authorization!.Parameter);
    }

    [Fact]
    public async Task AssignWalkwayAsync_PutsToAssignWalkwayRoute_WithWalkwayIdInBody()
    {
        const string responseJson = """{ "isSuccess": true, "message": "OK" }""";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var sut = CreateSut(handler);
        var userId = Guid.NewGuid();
        var walkwayId = Guid.NewGuid();

        var result = await sut.AssignWalkwayAsync(userId, walkwayId);

        Assert.True(result.IsSuccess);
        Assert.Equal(HttpMethod.Put, handler.LastRequest!.Method);
        Assert.EndsWith($"Users/AssignWalkway/{userId}", handler.LastRequest.RequestUri!.ToString());
        Assert.Contains(walkwayId.ToString(), handler.LastRequestBody);
    }

    [Fact]
    public async Task AssignWalkwayAsync_WithNullWalkwayId_SendsNull_ToClearAssignment()
    {
        const string responseJson = """{ "isSuccess": true, "message": "OK" }""";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var sut = CreateSut(handler);

        await sut.AssignWalkwayAsync(Guid.NewGuid(), null);

        Assert.Contains("\"walkwayId\":null", handler.LastRequestBody, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ChangeRoleAsync_PutsToChangeRoleRoute_WithRoleIdInBody()
    {
        const string responseJson = """{ "isSuccess": true, "message": "OK" }""";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.OK, responseJson);
        var sut = CreateSut(handler);
        var userId = Guid.NewGuid();
        var roleId = Guid.NewGuid();

        var result = await sut.ChangeRoleAsync(userId, roleId);

        Assert.True(result.IsSuccess);
        Assert.Equal(HttpMethod.Put, handler.LastRequest!.Method);
        Assert.EndsWith($"Users/ChangeRole/{userId}", handler.LastRequest.RequestUri!.ToString());
        Assert.Contains(roleId.ToString(), handler.LastRequestBody);
    }

    [Fact]
    public async Task ChangeRoleAsync_OnForbidden_ReturnsBackendMessage_NotGenericError()
    {
        // El backend devuelve 400 (o 403) con un body parseable cuando el permiso es insuficiente;
        // ApiService no debe reemplazarlo por un mensaje genérico de error de servidor.
        const string responseJson = """{ "isSuccess": false, "message": "La acción sobre 'Usuario' no está permitida o el estado es inválido." }""";
        var handler = new FakeHttpMessageHandler(HttpStatusCode.BadRequest, responseJson);
        var sut = CreateSut(handler);

        var result = await sut.ChangeRoleAsync(Guid.NewGuid(), Guid.NewGuid());

        Assert.False(result.IsSuccess);
        Assert.Equal("La acción sobre 'Usuario' no está permitida o el estado es inválido.", result.Message);
    }
}
