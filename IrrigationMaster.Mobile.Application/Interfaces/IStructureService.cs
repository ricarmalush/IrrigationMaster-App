using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.Application.Features.Models.Structure.Country;

namespace IrrigationMaster.Mobile.Application.Interfaces;

public interface IStructureService
{
    Task<(bool IsSuccess, string Message)> CreateOrganizationAsync(CreateOrganizationRequest request);
    Task<(bool IsSuccess, string Message)> CreateHydraulicSectorAsync(CreateHydraulicSectorRequest request);
    Task<(bool IsSuccess, string Message)> CreateWalkwayAsync(CreateWalkwayRequest request);
    Task<List<CountryDto>?> GetCountriesAsync();
}
