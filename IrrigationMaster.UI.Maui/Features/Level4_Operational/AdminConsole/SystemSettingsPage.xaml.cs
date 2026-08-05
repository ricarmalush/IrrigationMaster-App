namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.AdminConsole;

public partial class SystemSettingsPage : TabbedPage
{
    public SystemSettingsPage(SystemSettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Las pestañas se AÑADEN aquí -- nunca se QUITAN. Un intento anterior declaraba las 4
        // pestañas en el XAML y quitaba las que no tocaban con Children.Remove(), incluso desde
        // este mismo constructor (antes de Navigation.PushAsync). Eso seguía reventando en
        // Android con IllegalArgumentException ('No view found for id ...
        // navigationlayout_toptabs'): el adaptador de fragments del ViewPager nativo se inicializa
        // contando las 4 pestañas que el XAML compilado declaró, y un Remove() posterior --sin
        // importar en qué punto del ciclo de vida ocurra-- deja al adaptador con referencias a
        // ids que ya no existen. La única forma de evitar el bug de raíz es que las pestañas que
        // no tocan NUNCA lleguen a existir: por eso cada pestaña es ahora su propia ContentPage
        // (EntidadTabPage, SectoresTabPage, AndadoresTabPage, MiCuentaTabPage) y el TabbedPage
        // las añade condicionalmente según el rol (ya disponible de forma síncrona vía
        // ICurrentSession.CachedRole, poblado por EstablishAsync durante el login).
        if (viewModel.ShowEntidadTab)
        {
            Children.Add(new EntidadTabPage());
        }

        if (viewModel.ShowSectoresTab)
        {
            Children.Add(new SectoresTabPage());
        }

        if (viewModel.ShowAndadoresTab)
        {
            Children.Add(new AndadoresTabPage());
        }

        Children.Add(new MiCuentaTabPage());
    }

    private async void OnBackClicked(object? sender, EventArgs e)
    {
        await Navigation.PopModalAsync();
    }
}
