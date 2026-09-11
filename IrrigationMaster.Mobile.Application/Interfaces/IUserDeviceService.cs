using IrrigationMaster.Mobile.Application.Common.Dtos;

namespace IrrigationMaster.Mobile.Application.Interfaces;

// Autoservicio de dispositivos móviles (token push): registra o refresca el token del dispositivo
// actual contra el usuario autenticado. El backend hace upsert por token -- ver
// RegisterDeviceCommandHandler -- así que llamarlo varias veces con el mismo token es seguro.
public interface IUserDeviceService
{
    Task<StructureOperationResult> RegisterDeviceAsync(string deviceToken, string deviceModel, string osVersion);
}
