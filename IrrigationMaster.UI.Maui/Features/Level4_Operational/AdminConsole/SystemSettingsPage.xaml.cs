namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.AdminConsole;

public partial class SystemSettingsPage : TabbedPage
{
    public SystemSettingsPage(SystemSettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        // El backend ya rechaza con 403 la creación de Organización si el llamador no es
        // SUPERADMIN -- aquí solo evitamos mostrarle al Presidente una pestaña que nunca va a
        // funcionar para él. Se vuelve a consultar (en vez de fiarse del valor ya cargado por
        // el constructor del ViewModel) para no depender de una carrera con esa carga en segundo plano.
        if (BindingContext is SystemSettingsViewModel viewModel)
        {
            await viewModel.LoadCurrentUserRoleAsync();
            if (!viewModel.IsSuperAdmin)
            {
                Children.Remove(EntidadTab);
            }
        }
    }
}