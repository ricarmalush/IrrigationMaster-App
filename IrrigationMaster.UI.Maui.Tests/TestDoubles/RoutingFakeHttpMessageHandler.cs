using System.Net;
using System.Text;

namespace IrrigationMaster.UI.Maui.Tests.TestDoubles;

// Enruta cada request por predicado a una respuesta enlatada (o a una excepción, para
// simular un fallo de red). Lo que no coincide con ninguna ruta devuelve una lista vacía
// 200 OK -- es lo que reciben GetCountriesAsync/GetHydraulicSectorsAsync cuando el test
// no les presta atención, para no interferir con el flujo bajo prueba.
public class RoutingFakeHttpMessageHandler : HttpMessageHandler
{
    private sealed record Route(Func<HttpRequestMessage, bool> Match, HttpStatusCode StatusCode, string Content, bool Throws);

    private readonly List<Route> _routes = [];
    public List<HttpRequestMessage> Requests { get; } = [];

    public void AddRoute(Func<HttpRequestMessage, bool> match, HttpStatusCode statusCode, string content)
        => _routes.Add(new Route(match, statusCode, content, Throws: false));

    public void AddThrowingRoute(Func<HttpRequestMessage, bool> match)
        => _routes.Add(new Route(match, HttpStatusCode.OK, string.Empty, Throws: true));

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Requests.Add(request);

        var route = _routes.FirstOrDefault(r => r.Match(request));

        if (route?.Throws == true)
            throw new HttpRequestException("Fallo de red simulado.");

        var statusCode = route?.StatusCode ?? HttpStatusCode.OK;
        var content = route?.Content ?? """{"data":[],"totalCount":0,"pageNumber":1,"pageSize":200}""";

        return Task.FromResult(new HttpResponseMessage(statusCode)
        {
            Content = new StringContent(content, Encoding.UTF8, "application/json")
        });
    }

    public static Func<HttpRequestMessage, bool> IsPostTo(string pathSuffix)
        => r => r.Method == HttpMethod.Post && r.RequestUri!.AbsolutePath.EndsWith(pathSuffix, StringComparison.OrdinalIgnoreCase);

    public static Func<HttpRequestMessage, bool> IsGetTo(string pathSuffix)
        => r => r.Method == HttpMethod.Get && r.RequestUri!.AbsolutePath.EndsWith(pathSuffix, StringComparison.OrdinalIgnoreCase);

    public static Func<HttpRequestMessage, bool> IsPutTo(string pathSuffix)
        => r => r.Method == HttpMethod.Put && r.RequestUri!.AbsolutePath.EndsWith(pathSuffix, StringComparison.OrdinalIgnoreCase);
}
