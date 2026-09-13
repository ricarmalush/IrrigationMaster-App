using IrrigationMaster.Mobile.Application.Interfaces;

namespace IrrigationMaster.UI.Maui.Services;

/// <summary>
/// Guarda el PDF en el caché de la App y deja que el sistema operativo lo abra con el visor de
/// PDF que tenga instalado -- no hay precedente de descarga de fichero en esta App, y así evitamos
/// implementar un visor propio para un caso de uso tan puntual (ver MyInvoicesViewModel).
/// </summary>
public sealed class MauiReceiptOpener : IReceiptOpener
{
    public async Task OpenAsync(string fileName, byte[] content)
    {
        var path = Path.Combine(FileSystem.CacheDirectory, fileName);
        await File.WriteAllBytesAsync(path, content);

        await Launcher.Default.OpenAsync(new OpenFileRequest(fileName, new ReadOnlyFile(path)));
    }
}
