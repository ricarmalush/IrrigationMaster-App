using IrrigationMaster.Mobile.Application.Common.Dtos;
using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.Application.Features.Models.Structure.Country;

namespace IrrigationMaster.Mobile.Application.Interfaces;

public interface IStructureService
{
    Task<StructureOperationResult> CreateOrganizationAsync(CreateOrganizationRequest request);
    Task<StructureOperationResult> CreateHydraulicSectorAsync(CreateHydraulicSectorRequest request);
    Task<StructureOperationResult> CreateWalkwayAsync(CreateWalkwayRequest request);
    Task<List<CountryDto>?> GetCountriesAsync();
    Task<List<HydraulicSectorDto>?> GetHydraulicSectorsAsync();
    Task<OrganizationDto?> GetOrganizationAsync(Guid organizationId);

    // Listado completo de organizaciones (Organizations/pagination). El backend ya acota esto
    // por sí solo: SUPERADMIN ve todas, cualquier otro autenticado solo la suya -- esta llamada
    // solo se usa hoy desde el desplegable de filtro de UserManagementViewModel, visible solo
    // para SUPERADMIN.
    Task<List<OrganizationDto>?> GetOrganizationsAsync();
}
