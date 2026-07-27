using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrrigationMaster.Mobile.Core.Features.Models.Structure
{
    public class CreateWalkwayRequest
    {
        public string Code { get; set; } = string.Empty;
        public decimal Length { get; set; }
        public Guid HydraulicSectorId { get; set; } // El sector hidráulico al que pertenece
    }
}
