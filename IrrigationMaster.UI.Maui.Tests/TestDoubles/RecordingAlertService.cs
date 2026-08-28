using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class RecordingAlertService : IAlertService
{
    public List<(string Title, string Message)> Calls { get; } = [];
    public List<(string Title, string Message, string AcceptText, string CancelText)> ConfirmCalls { get; } = [];

    // Lo que debe devolver ShowConfirmAsync la próxima vez que se llame -- por defecto true
    // (el usuario confirma), para no obligar a todos los tests existentes a configurarlo.
    public bool ConfirmResult { get; set; } = true;

    public Task ShowAsync(string title, string message)
    {
        Calls.Add((title, message));
        return Task.CompletedTask;
    }

    public Task<bool> ShowConfirmAsync(string title, string message, string acceptText, string cancelText)
    {
        ConfirmCalls.Add((title, message, acceptText, cancelText));
        return Task.FromResult(ConfirmResult);
    }
}
