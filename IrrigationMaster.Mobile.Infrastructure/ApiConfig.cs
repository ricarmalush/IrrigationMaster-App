namespace IrrigationMaster.Mobile.Infrastructure;

public static class ApiConfig
{
#if DEBUG
    public const string BaseUrl = "https://pcn181v8-44384.euw.devtunnels.ms/api/v1/";
#else
    // Sin backend de producción desplegado todavía: usa el mismo Dev Tunnel que Debug.
    // Reemplazar por la URL real antes de publicar un build Release.
    public const string BaseUrl = "https://pcn181v8-44384.euw.devtunnels.ms/api/v1/";
#endif
}
