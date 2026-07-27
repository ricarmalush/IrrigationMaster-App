using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class RecordingNavigationService : INavigationService
{
    public List<string> Routes { get; } = [];

    public Task GoToAsync(string route)
    {
        Routes.Add(route);
        return Task.CompletedTask;
    }
}
