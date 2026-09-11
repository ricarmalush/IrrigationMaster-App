using IrrigationMaster.Mobile.Application.Common.Dtos;
using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class FakeUserDeviceService : IUserDeviceService
{
    public StructureOperationResult ResultToReturn { get; set; } = new() { IsSuccess = true, Data = Guid.NewGuid() };
    public (string DeviceToken, string DeviceModel, string OsVersion)? LastCall { get; private set; }

    public Task<StructureOperationResult> RegisterDeviceAsync(string deviceToken, string deviceModel, string osVersion)
    {
        LastCall = (deviceToken, deviceModel, osVersion);
        return Task.FromResult(ResultToReturn);
    }
}
