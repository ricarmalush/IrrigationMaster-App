using IrrigationMaster.Domain.Level2_Structure;

namespace IrrigationMaster.Mobile.UnitTests.Domain;

public class WalkwayTests
{
    [Fact]
    public void Constructor_WithValidData_SetsProperties()
    {
        var id = Guid.NewGuid();
        var sectorId = Guid.NewGuid();

        var walkway = new Walkway(id, "A-01", 400.5m, sectorId);

        Assert.Equal(id, walkway.Id);
        Assert.Equal("A-01", walkway.Code);
        Assert.Equal(400.5m, walkway.Length);
        Assert.Equal(sectorId, walkway.HydraulicSectorId);
    }

    [Fact]
    public void Constructor_WithEmptyId_Throws()
        => Assert.Throws<ArgumentException>(() => new Walkway(Guid.Empty, "A-01", 100m, Guid.NewGuid()));

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithInvalidCode_Throws(string? code)
        => Assert.Throws<ArgumentException>(() => new Walkway(Guid.NewGuid(), code!, 100m, Guid.NewGuid()));

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    public void Constructor_WithNonPositiveLength_Throws(decimal length)
        => Assert.Throws<ArgumentException>(() => new Walkway(Guid.NewGuid(), "A-01", length, Guid.NewGuid()));

    [Fact]
    public void Constructor_WithEmptyHydraulicSectorId_Throws()
        => Assert.Throws<ArgumentException>(() => new Walkway(Guid.NewGuid(), "A-01", 100m, Guid.Empty));
}
