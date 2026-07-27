namespace IrrigationMaster.Mobile.Infrastructure;

// Identidad del cliente actual de este build (hoy: El Saso). El día que esta app se venda a otro
// cliente, cambia este valor y solo este — ningún ViewModel ni Command lo conoce.
// Separado de ApiConfig (esa es conectividad; esto es identidad de tenant).
//
// DefaultOrganizationId ya no existe aquí: el registro anónimo dejó de enviar OrganizationId
// directamente (ver RegisterViewModel/CreateUserRequest) -- ahora el vecino escribe el código de
// invitación de su comunidad y el backend resuelve la organización a partir de ese código.
// DefaultVecinoRoleId sigue siendo válido: el rol VECINO es global y fijo, no depende de la
// organización elegida.
public static class TenantConfig
{
#if DEBUG
    public static readonly Guid DefaultVecinoRoleId = new("c5eeb480-9acf-444f-8a63-1fb7ec93e864");
#else
    // Mismo valor que Debug hasta que exista un despliegue de producción distinto.
    public static readonly Guid DefaultVecinoRoleId = new("c5eeb480-9acf-444f-8a63-1fb7ec93e864");
#endif
}
