namespace IrrigationMaster.Mobile.Infrastructure;

// Identidad del cliente actual de este build (hoy: El Saso). El día que esta app se venda a otro
// cliente, cambian estos dos valores y solo estos — ningún ViewModel ni Command los conoce.
// Separado de ApiConfig (esa es conectividad; esto es identidad de tenant).
public static class TenantConfig
{
#if DEBUG
    public static readonly Guid DefaultOrganizationId = new("85dd6e16-805c-4729-90aa-76c9fcfe406c");
    public static readonly Guid DefaultVecinoRoleId = new("c5eeb480-9acf-444f-8a63-1fb7ec93e864");
#else
    // Mismos valores que Debug hasta que exista un despliegue de producción distinto para El Saso.
    public static readonly Guid DefaultOrganizationId = new("85dd6e16-805c-4729-90aa-76c9fcfe406c");
    public static readonly Guid DefaultVecinoRoleId = new("c5eeb480-9acf-444f-8a63-1fb7ec93e864");
#endif
}
