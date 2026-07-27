using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrrigationMaster.Mobile.Application.Constants
{
    public static class ServiceMessages
    {
        public const string ServerErrorCode = "Error del servidor (Código:";
        public const string ApiConnectionError = "Fallo de conexión con el servidor.";

        public const string NetworkConnectionError = "No se pudo establecer comunicación con el servidor. Verifica tu conexión a internet.";
        public const string UnexpectedError = "Ocurrió un error inesperado al procesar la solicitud.";

        public const string OrgCreatedSuccess = "Organización creada con éxito.";
        public const string OrgCreatedError = "Error al crear la organización en el servidor.";

        public const string SectorCreatedSuccess = "Sector Hidráulico creado con éxito.";
        public const string SectorCreatedError = "Error al crear el sector hidráulico.";

        public const string WalkwayCreatedSuccess = "Andador registrado con éxito.";
        public const string WalkwayCreatedError = "Error al registrar el andador.";
    }
}
