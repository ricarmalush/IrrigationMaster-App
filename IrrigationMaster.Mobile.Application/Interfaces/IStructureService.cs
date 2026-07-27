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
}
