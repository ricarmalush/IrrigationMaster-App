namespace IrrigationMaster.Mobile.Application.Interfaces;

// Abstrae "guardar el PDF y abrirlo con el visor del sistema" para que el ViewModel sea testeable
// sin necesitar una App MAUI corriendo (FileSystem.CacheDirectory/Launcher son estáticos de
// plataforma, no inyectables directamente) -- mismo motivo que INavigationService para Shell.
public interface IReceiptOpener
{
    Task OpenAsync(string fileName, byte[] content);
}
