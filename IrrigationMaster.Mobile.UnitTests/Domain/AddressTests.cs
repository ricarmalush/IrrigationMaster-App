using IrrigationMaster.Domain.Level2_Structure.ValueObjects;

namespace IrrigationMaster.Mobile.UnitTests.Domain;

public class AddressTests
{
    [Fact]
    public void Constructor_WithValidData_SetsProperties()
    {
        var countryId = Guid.NewGuid();

        var address = new Address("Camino Real s/n", "El Saso", "Huesca", "22300", countryId, "Junto a la ermita");

        Assert.Equal("Camino Real s/n", address.MainAddress);
        Assert.Equal("El Saso", address.City);
        Assert.Equal("Huesca", address.StateOrProvince);
        Assert.Equal("22300", address.PostalCode);
        Assert.Equal(countryId, address.CountryId);
        Assert.Equal("Junto a la ermita", address.LocationDetail);
    }

    [Fact]
    public void Constructor_WithoutLocationDetail_AllowsNull()
    {
        var address = new Address("Calle Mayor", "Zaragoza", "Zaragoza", "50001", Guid.NewGuid());

        Assert.Null(address.LocationDetail);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidMainAddress_Throws(string? mainAddress)
        => Assert.Throws<ArgumentException>(() => new Address(mainAddress!, "Ciudad", "Provincia", "12345", Guid.NewGuid()));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCity_Throws(string? city)
        => Assert.Throws<ArgumentException>(() => new Address("Calle", city!, "Provincia", "12345", Guid.NewGuid()));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidStateOrProvince_Throws(string? stateOrProvince)
        => Assert.Throws<ArgumentException>(() => new Address("Calle", "Ciudad", stateOrProvince!, "12345", Guid.NewGuid()));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidPostalCode_Throws(string? postalCode)
        => Assert.Throws<ArgumentException>(() => new Address("Calle", "Ciudad", "Provincia", postalCode!, Guid.NewGuid()));

    [Fact]
    public void Constructor_WithEmptyCountryId_Throws()
        => Assert.Throws<ArgumentException>(() => new Address("Calle", "Ciudad", "Provincia", "12345", Guid.Empty));

    [Fact]
    public void TwoAddresses_WithSameValues_AreEqual()
    {
        var countryId = Guid.NewGuid();
        var a = new Address("Calle", "Ciudad", "Provincia", "12345", countryId);
        var b = new Address("Calle", "Ciudad", "Provincia", "12345", countryId);

        Assert.Equal(a, b);
    }

    [Fact]
    public void TwoAddresses_WithDifferentValues_AreNotEqual()
    {
        var countryId = Guid.NewGuid();
        var a = new Address("Calle A", "Ciudad", "Provincia", "12345", countryId);
        var b = new Address("Calle B", "Ciudad", "Provincia", "12345", countryId);

        Assert.NotEqual(a, b);
    }
}
