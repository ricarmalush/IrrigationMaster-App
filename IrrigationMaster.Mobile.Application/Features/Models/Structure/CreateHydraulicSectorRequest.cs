namespace IrrigationMaster.Mobile.Application.Features.Models.Structure
{
    public class CreateHydraulicSectorRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal AreaSize { get; set; } // Representado en Hectáreas
        // El backend resuelve la organización del JWT vía ICurrentUser: no va en el body.
    }
}
