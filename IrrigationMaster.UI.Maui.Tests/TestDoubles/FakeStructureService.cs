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

    // Clave: WalkwayId consultado -> detalle a devolver. Permite a IrrigationStatusViewModelTests
    // configurar el HydraulicSectorId de cada andador vacío sin tener que implementar todo el
    // resto de IStructureService.
    public Dictionary<Guid, WalkwayDetailDto> WalkwaysById { get; set; } = [];
    public Guid? LastGetWalkwayCall { get; private set; }

    public Task<List<OrganizationDto>?> GetOrganizationsAsync() => Task.FromResult(OrganizationsToReturn);

    public Task<WalkwayDetailDto?> GetWalkwayAsync(Guid walkwayId)
    {
        LastGetWalkwayCall = walkwayId;
        return Task.FromResult(WalkwaysById.GetValueOrDefault(walkwayId));
    }

    public Task<StructureOperationResult> CreateOrganizationAsync(CreateOrganizationRequest request) => throw new NotImplementedException();
    public Task<StructureOperationResult> CreateHydraulicSectorAsync(CreateHydraulicSectorRequest request) => throw new NotImplementedException();
    public Task<StructureOperationResult> CreateWalkwayAsync(CreateWalkwayRequest request) => throw new NotImplementedException();
    public Task<List<CountryDto>?> GetCountriesAsync() => throw new NotImplementedException();
    public Task<List<HydraulicSectorDto>?> GetHydraulicSectorsAsync() => throw new NotImplementedException();
    public Task<OrganizationDto?> GetOrganizationAsync(Guid organizationId) => throw new NotImplementedException();
}
