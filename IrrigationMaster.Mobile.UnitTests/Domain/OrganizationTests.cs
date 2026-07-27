using IrrigationMaster.Domain.Level2_Structure;
using IrrigationMaster.Domain.Level2_Structure.ValueObjects;

namespace IrrigationMaster.Mobile.UnitTests.Domain;

public class OrganizationTests
{
    private static Address ValidAddress() => new(
        mainAddress: "Camino Real s/n",
        city: "El Saso",
        stateOrProvince: "Huesca",
        postalCode: "22300",
        countryId: Guid.NewGuid());

    [Fact]
    public void Constructor_WithValidData_SetsProperties()
    {
        var id = Guid.NewGuid();
        var address = ValidAddress();

        var organization = new Organization(id, "Regantes El Saso", "G50123456", address);

        Assert.Equal(id, organization.Id);
        Assert.Equal("Regantes El Saso", organization.Name);
        Assert.Equal("G50123456", organization.TaxId);
        Assert.Equal(address, organization.Address);
    }

    [Fact]
    public void Constructor_WithEmptyId_Throws()
        => Assert.Throws<ArgumentException>(() => new Organization(Guid.Empty, "Nombre", "TaxId", ValidAddress()));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidName_Throws(string? name)
        => Assert.Throws<ArgumentException>(() => new Organization(Guid.NewGuid(), name!, "TaxId", ValidAddress()));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidTaxId_Throws(string? taxId)
        => Assert.Throws<ArgumentException>(() => new Organization(Guid.NewGuid(), "Nombre", taxId!, ValidAddress()));

    [Fact]
    public void Constructor_WithNullAddress_Throws()
        => Assert.Throws<ArgumentNullException>(() => new Organization(Guid.NewGuid(), "Nombre", "TaxId", null!));
}
