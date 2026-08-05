using System.ComponentModel;
using System.Windows.Input;

namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.AdminConsole;

public partial class SystemSettingsPage : ContentPage
{
    private const string ActiveTextColor = "White";
    private const string InactiveTextColor = "#A5D6A7";

    private readonly Dictionary<SettingsTab, Button> _tabButtons = [];

    public SystemSettingsPage(SystemSettingsViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;

        // Tira de pestañas construida aquí, no en XAML: solo debe existir un botón por cada
        // pestaña que el rol puede ver (mismo motivo que ya se aplicaba a las Views de
        // contenido), y para que se repartan el ancho a partes iguales entre solo las
        // visibles, sin huecos -- Grid.ColumnDefinitions no se puede enlazar dinámicamente a
        // la cantidad de pestañas visibles desde XAML.
        AddTab("ENTIDAD", SettingsTab.Entidad, viewModel.ShowEntidadTab, viewModel.SelectEntidadTabCommand);
        AddTab("SECTORES", SettingsTab.Sectores, viewModel.ShowSectoresTab, viewModel.SelectSectoresTabCommand);
        AddTab("ANDADORES", SettingsTab.Andadores, viewModel.ShowAndadoresTab, viewModel.SelectAndadoresTabCommand);
        AddTab("MI CUENTA", SettingsTab.MiCuenta, visible: true, viewModel.SelectMiCuentaTabCommand);

        // Cada View de contenido decide su propia visibilidad por sí misma (IsVisible enlazado a
        // Is*TabActive en su XAML); aquí solo se decide cuáles llegan a existir, igual que con
        // las pestañas: nunca crear lo que el rol no puede ver.
        if (viewModel.ShowEntidadTab)
        {
            ContentArea.Children.Add(new EntidadTabView { BindingContext = viewModel });
        }

        if (viewModel.ShowSectoresTab)
        {
            ContentArea.Children.Add(new SectoresTabView { BindingContext = viewModel });
        }

        if (viewModel.ShowAndadoresTab)
        {
            ContentArea.Children.Add(new AndadoresTabView { BindingContext = viewModel });
        }

        ContentArea.Children.Add(new MiCuentaTabView { BindingContext = viewModel });

        viewModel.PropertyChanged += OnViewModelPropertyChanged;
        RefreshTabButtonStyles(viewModel.ActiveTab);
    }

    private void AddTab(string text, SettingsTab tab, bool visible, ICommand command)
    {
        if (!visible)
        {
            return;
        }

        TabStripGrid.ColumnDefinitions.Add(new ColumnDefinition(GridLength.Star));

        var button = new Button
        {
            Text = text,
            Command = command,
            BackgroundColor = Colors.Transparent,
            TextColor = Color.FromArgb(InactiveTextColor),
            FontAttributes = FontAttributes.Bold,
            FontSize = 13,
            CornerRadius = 0,
            Padding = new Thickness(4, 0)
        };

        Grid.SetColumn(button, TabStripGrid.ColumnDefinitions.Count - 1);
        TabStripGrid.Children.Add(button);
        _tabButtons[tab] = button;
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(SystemSettingsViewModel.ActiveTab) && BindingContext is SystemSettingsViewModel viewModel)
        {
            RefreshTabButtonStyles(viewModel.ActiveTab);
        }
    }

    private void RefreshTabButtonStyles(SettingsTab activeTab)
    {
        foreach (var (tab, button) in _tabButtons)
        {
            button.TextColor = tab == activeTab ? Color.FromArgb(ActiveTextColor) : Color.FromArgb(InactiveTextColor);
        }
    }
}
