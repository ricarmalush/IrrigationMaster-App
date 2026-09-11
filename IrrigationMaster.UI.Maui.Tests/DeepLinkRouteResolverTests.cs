using IrrigationMaster.Mobile.Application.Features.Models.Devices;
using IrrigationMaster.UI.Maui.Services;

namespace IrrigationMaster.UI.Maui.Tests;

public class DeepLinkRouteResolverTests
{
    [Fact]
    public void Resolve_WithRouteKey_ReturnsRoute()
    {
        var notification = new PushNotificationInfo
        {
            Title = "Nueva incidencia",
            Body = "Se ha reportado una incidencia en tu andador",
            Data = new Dictionary<string, string> { ["route"] = "//AdminMenuPage" }
        };

        Assert.Equal("//AdminMenuPage", DeepLinkRouteResolver.Resolve(notification));
    }

    [Fact]
    public void Resolve_WithoutRouteKey_ReturnsNull()
    {
        var notification = new PushNotificationInfo { Title = "Aviso", Body = "Sin ruta asociada" };

        Assert.Null(DeepLinkRouteResolver.Resolve(notification));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Resolve_WithBlankRouteValue_ReturnsNull(string blankRoute)
    {
        var notification = new PushNotificationInfo
        {
            Data = new Dictionary<string, string> { ["route"] = blankRoute }
        };

        Assert.Null(DeepLinkRouteResolver.Resolve(notification));
    }
}
