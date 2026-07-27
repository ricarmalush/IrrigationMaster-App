namespace IrrigationMaster.UI.Maui
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("RegisterPage", typeof(Features.Level1_Core.Register.RegisterPage));
            Routing.RegisterRoute("AdminMenuPage", typeof(Features.Level4_Operational.AdminConsole.AdminMenuPage));
        }
    }
}
