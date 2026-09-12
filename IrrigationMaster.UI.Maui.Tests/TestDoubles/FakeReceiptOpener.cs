using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

public class FakeReceiptOpener : IReceiptOpener
{
    public List<(string FileName, byte[] Content)> Calls { get; } = [];
    public Exception? ExceptionToThrow { get; set; }

    public Task OpenAsync(string fileName, byte[] content)
    {
        if (ExceptionToThrow != null) throw ExceptionToThrow;

        Calls.Add((fileName, content));
        return Task.CompletedTask;
    }
}
