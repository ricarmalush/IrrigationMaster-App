using IrrigationMaster.Domain.Level2_Structure.ValueObjects;

namespace IrrigationMaster.Domain.Level2_Structure;

public sealed class Organization
{
    public Guid Id { get; }
    public string Name { get; }
    public string TaxId { get; }
    public Address Address { get; }

    public Organization(Guid id, string name, string taxId, Address address)
    {
        if (id == Guid.Empty) throw new ArgumentException("El identificador es obligatorio.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("El nombre es obligatorio.", nameof(name));
        if (string.IsNullOrWhiteSpace(taxId)) throw new ArgumentException("El NIF/CIF es obligatorio.", nameof(taxId));

        Id = id;
        Name = name;
        TaxId = taxId;
        Address = address ?? throw new ArgumentNullException(nameof(address));
    }
}
