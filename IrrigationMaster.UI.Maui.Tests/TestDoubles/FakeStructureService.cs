using IrrigationMaster.Mobile.Application.Common.Dtos;
using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.Application.Features.Models.Structure.Country;
using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

// Solo implementa lo que hoy usan los tests que la consumen (desplegable de organización de
// UserManagementViewModel). El resto de miembros de IStructureService no se usa fuera de
// SystemSettingsViewModel, que ya tiene su propia cobertura vía ApiService+RoutingFakeHttpMessageHandler.
public class FakeStructureService : IStructureService
{
    public List<OrganizationDto>? OrganizationsToReturn { get; set; } = [];

    public Task<List<OrganizationDto>?> GetOrganizationsAsync() => Task.FromResult(OrganizationsToReturn);

    public Task<StructureOperationResult> CreateOrganizationAsync(CreateOrganizationRequest request) => throw new NotImplementedException();
    public Task<StructureOperationResult> CreateHydraulicSectorAsync(CreateHydraulicSectorRequest request) => throw new NotImplementedException();
    public Task<StructureOperationResult> CreateWalkwayAsync(CreateWalkwayRequest request) => throw new NotImplementedException();
    public Task<List<CountryDto>?> GetCountriesAsync() => throw new NotImplementedException();
    public Task<List<HydraulicSectorDto>?> GetHydraulicSectorsAsync() => throw new NotImplementedException();
    public Task<OrganizationDto?> GetOrganizationAsync(Guid organizationId) => throw new NotImplementedException();
}
