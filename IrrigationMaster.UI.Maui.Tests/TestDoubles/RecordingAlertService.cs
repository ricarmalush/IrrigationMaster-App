using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class RecordingAlertService : IAlertService
{
    public List<(string Title, string Message)> Calls { get; } = [];

    public Task ShowAsync(string title, string message)
    {
        Calls.Add((title, message));
        return Task.CompletedTask;
    }
}
