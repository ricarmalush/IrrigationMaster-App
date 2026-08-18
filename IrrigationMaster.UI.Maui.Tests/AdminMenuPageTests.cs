using IrrigationMaster.UI.Maui.Features.Level4_Operational.AdminConsole;
using Xunit;

namespace IrrigationMaster.UI.Maui.Tests;

public class AdminMenuPageTests
{
    [Fact]
    public void BuildHeaderText_WithOrganizationName_AppendsIt()
    {
        Assert.Equal("Gestión Operativa - Cooperativa Horizonte Verde",
            AdminMenuPage.BuildHeaderText("Cooperativa Horizonte Verde"));
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void BuildHeaderText_WithoutOrganizationName_FallsBackToGenericText(string? organizationName)
    {
        // Antes de que GetOrganizationAsync responda (o si falla), nunca debe mostrarse un nombre
        // de cliente fijo -- el texto genérico sin sufijo es el único fallback aceptable.
        Assert.Equal("Gestión Operativa", AdminMenuPage.BuildHeaderText(organizationName));
    }
}
