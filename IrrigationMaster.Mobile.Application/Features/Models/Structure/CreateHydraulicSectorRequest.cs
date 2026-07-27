using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrrigationMaster.Mobile.Application.Features.Models.Structure
{
    public class CreateHydraulicSectorRequest
    {
        public string Name { get; set; } = string.Empty;
        public decimal AreaSize { get; set; } // Representado en Hectáreas
        public Guid OrganizationId { get; set; } // Vínculo obligatorio a la organización
    }
}
