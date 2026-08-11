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

    // Detalle de un andador concreto (Walkways/Get/{id}), con su HydraulicSectorId -- a diferencia
    // de WalkwayDto (solo Id+Code, usado en los Picker de asignación). Hoy solo lo usa
    // IrrigationStatusViewModel: para consultar IrrigationPrograms/IsIrrigationDay sobre un
    // andador sin ningún vecino con turno hoy, necesita saber a qué sector pertenece.
    Task<WalkwayDetailDto?> GetWalkwayAsync(Guid walkwayId);
}
