namespace IrrigationMaster.Mobile.Application.Features.Models.Structure;

public class AddressRequest
{
    public string MainAddress { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string StateOrProvince { get; set; } = string.Empty;
    public string PostalCode { get; set; } = string.Empty;
    public Guid CountryId { get; set; }
    public string? LocationDetail { get; set; }
}
