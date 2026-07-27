using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IrrigationMaster.Mobile.Core.Features.Models.Structure
{
    public class CreateOrganizationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;

        // Desglosamos el Value Object Address para los Entry de la UI
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string ZipCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
    }
}
