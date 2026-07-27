namespace IrrigationMaster.Domain.Level2_Structure;

public sealed class Walkway
{
    public Guid Id { get; }
    public string Code { get; }
    public decimal Length { get; }
    public Guid HydraulicSectorId { get; }

    public Walkway(Guid id, string code, decimal length, Guid hydraulicSectorId)
    {
        if (id == Guid.Empty) throw new ArgumentException("El identificador es obligatorio.", nameof(id));
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("El código es obligatorio.", nameof(code));
        if (length <= 0) throw new ArgumentException("La longitud debe ser mayor que cero.", nameof(length));
        if (hydraulicSectorId == Guid.Empty) throw new ArgumentException("El sector hidráulico es obligatorio.", nameof(hydraulicSectorId));

        Id = id;
        Code = code;
        Length = length;
        HydraulicSectorId = hydraulicSectorId;
    }
}
