using IrrigationMaster.Mobile.Application.Common.Dtos;
using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class FakeRegistrationService : IRegistrationService
{
    public StructureOperationResult ResponseToReturn { get; set; } = new() { IsSuccess = true };
    public List<PublicWalkwayDto>? WalkwaysToReturn { get; set; }
    public CreateUserRequest? LastRequest { get; private set; }

    public Task<StructureOperationResult> RegisterAsync(CreateUserRequest request)
    {
        LastRequest = request;
        return Task.FromResult(ResponseToReturn);
    }

    public Task<List<PublicWalkwayDto>?> GetPublicWalkwaysAsync(Guid organizationId)
        => Task.FromResult(WalkwaysToReturn);
}
