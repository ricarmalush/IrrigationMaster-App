namespace IrrigationMaster.Mobile.Application.Features.Models.Structure
{
    public class CreateOrganizationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string TaxId { get; set; } = string.Empty;
        public AddressRequest Address { get; set; } = new();
    }
}
