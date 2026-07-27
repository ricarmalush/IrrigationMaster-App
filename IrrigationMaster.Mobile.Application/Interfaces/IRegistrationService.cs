using IrrigationMaster.Mobile.Application.Common.Dtos;
using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.Application.Features.Models.Users;

namespace IrrigationMaster.Mobile.Application.Interfaces;

// Operaciones anónimas (sin sesión) del flujo de auto-registro de usuarios.
public interface IRegistrationService
{
    Task<StructureOperationResult> RegisterAsync(CreateUserRequest request);
    Task<List<PublicWalkwayDto>?> GetPublicWalkwaysAsync(Guid organizationId);
}
