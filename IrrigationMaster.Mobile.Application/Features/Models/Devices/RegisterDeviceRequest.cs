namespace IrrigationMaster.Mobile.Application.Features.Models.Devices;

// Espejo de RegisterDeviceCommand del backend (UserDevices/Register). Autoservicio puro: el
// usuario objetivo se resuelve del JWT en el backend, nunca viaja en el body.
public class RegisterDeviceRequest
{
    public string DeviceToken { get; set; } = string.Empty;
    public string DeviceModel { get; set; } = string.Empty;
    public string OsVersion { get; set; } = string.Empty;
}
