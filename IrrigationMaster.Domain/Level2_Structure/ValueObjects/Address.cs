namespace IrrigationMaster.Domain.Level2_Structure.ValueObjects;

public sealed record Address
{
    public string MainAddress { get; }
    public string City { get; }
    public string StateOrProvince { get; }
    public string PostalCode { get; }
    public Guid CountryId { get; }
    public string? LocationDetail { get; }

    public Address(string mainAddress, string city, string stateOrProvince, string postalCode, Guid countryId, string? locationDetail = null)
    {
        if (string.IsNullOrWhiteSpace(mainAddress)) throw new ArgumentException("La dirección principal es obligatoria.", nameof(mainAddress));
        if (string.IsNullOrWhiteSpace(city)) throw new ArgumentException("La ciudad es obligatoria.", nameof(city));
        if (string.IsNullOrWhiteSpace(stateOrProvince)) throw new ArgumentException("La provincia/estado es obligatoria.", nameof(stateOrProvince));
        if (string.IsNullOrWhiteSpace(postalCode)) throw new ArgumentException("El código postal es obligatorio.", nameof(postalCode));
        if (countryId == Guid.Empty) throw new ArgumentException("El país es obligatorio.", nameof(countryId));

        MainAddress = mainAddress;
        City = city;
        StateOrProvince = stateOrProvince;
        PostalCode = postalCode;
        CountryId = countryId;
        LocationDetail = locationDetail;
    }
}
