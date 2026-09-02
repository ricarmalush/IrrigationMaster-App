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

    // ─── MENÚ POR ROL: cada botón enumera sus propios roles, ya no un único flag "no es Vecino" ───

    [Fact]
    public void ComputeMenuVisibility_SuperAdmin_SeesEverything()
    {
        var visibility = AdminMenuPage.ComputeMenuVisibility("SUPERADMIN");

        Assert.True(visibility.ShowUserManagement);
        Assert.True(visibility.ShowCommunityBroadcast);
        Assert.True(visibility.ShowApproveTurns);
        Assert.True(visibility.ShowIrrigationPrograms);
    }

    [Theory]
    [InlineData("PRESIDENTE")]
    [InlineData("VICEPRESIDENTE")]
    public void ComputeMenuVisibility_OrganizationAuthority_SeesUserManagementAndApproveTurns_ButNotIrrigationPrograms(string role)
    {
        var visibility = AdminMenuPage.ComputeMenuVisibility(role);

        Assert.True(visibility.ShowUserManagement);
        Assert.True(visibility.ShowCommunityBroadcast);
        Assert.True(visibility.ShowApproveTurns);
        Assert.False(visibility.ShowIrrigationPrograms);
    }

    [Fact]
    public void ComputeMenuVisibility_CoordinadorRiego_SeesBroadcastAndIrrigationPrograms_ButNotUserManagementOrApproveTurns()
    {
        var visibility = AdminMenuPage.ComputeMenuVisibility("COORDINADOR_RIEGO");

        Assert.False(visibility.ShowUserManagement);
        Assert.True(visibility.ShowCommunityBroadcast);
        Assert.False(visibility.ShowApproveTurns);
        Assert.True(visibility.ShowIrrigationPrograms);
    }

    [Theory]
    [InlineData("VECINO")]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("SOME_UNKNOWN_ROLE")]
    public void ComputeMenuVisibility_VecinoOrUnknownRole_SeesNothingGated(string? role)
    {
        var visibility = AdminMenuPage.ComputeMenuVisibility(role);

        Assert.False(visibility.ShowUserManagement);
        Assert.False(visibility.ShowCommunityBroadcast);
        Assert.False(visibility.ShowApproveTurns);
        Assert.False(visibility.ShowIrrigationPrograms);
    }

    [Fact]
    public void ComputeMenuVisibility_RoleComparison_IsCaseInsensitive()
    {
        var visibility = AdminMenuPage.ComputeMenuVisibility("coordinador_riego");

        Assert.True(visibility.ShowIrrigationPrograms);
    }

    // ─── "Estado de Riego" para Vecino: pasa a apuntar a MyIrrigationPage, "Mi Riego" se oculta ───

    [Fact]
    public void ComputeMenuVisibility_Vecino_HidesMyIrrigation_AndRedirectsIrrigationStatusToMyWalkway()
    {
        var visibility = AdminMenuPage.ComputeMenuVisibility("VECINO");

        Assert.False(visibility.ShowMyIrrigation);
        Assert.True(visibility.IrrigationStatusGoesToMyWalkway);
    }

    [Theory]
    [InlineData("SUPERADMIN")]
    [InlineData("PRESIDENTE")]
    [InlineData("VICEPRESIDENTE")]
    [InlineData("COORDINADOR_RIEGO")]
    public void ComputeMenuVisibility_NonVecino_ShowsMyIrrigation_AndDoesNotRedirectIrrigationStatus(string role)
    {
        var visibility = AdminMenuPage.ComputeMenuVisibility(role);

        Assert.True(visibility.ShowMyIrrigation);
        Assert.False(visibility.IrrigationStatusGoesToMyWalkway);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("SOME_UNKNOWN_ROLE")]
    public void ComputeMenuVisibility_NullOrUnknownRole_ShowsMyIrrigation_AndDoesNotRedirectIrrigationStatus(string? role)
    {
        // Sin rol resuelto todavía (o un rol desconocido), no se asume Vecino -- ambos botones se
        // comportan como para el resto de roles hasta que el rol real llegue.
        var visibility = AdminMenuPage.ComputeMenuVisibility(role);

        Assert.True(visibility.ShowMyIrrigation);
        Assert.False(visibility.IrrigationStatusGoesToMyWalkway);
    }
}
