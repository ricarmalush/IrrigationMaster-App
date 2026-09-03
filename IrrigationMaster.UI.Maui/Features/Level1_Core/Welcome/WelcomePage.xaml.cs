namespace IrrigationMaster.UI.Maui.Features.Level1_Core.Welcome;

public partial class WelcomePage : ContentPage
{
    private readonly WelcomeViewModel _viewModel;

    // Evita repetir la animación (y la navegación automática) si OnAppearing se disparase más de
    // una vez -- no debería ocurrir en el flujo normal (se navega con ruta absoluta "//", que
    // reemplaza la pila), pero es una guarda barata.
    private bool _hasAnimated;

    public WelcomePage(WelcomeViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_hasAnimated)
            return;
        _hasAnimated = true;

        await PlayEntranceAnimationAsync();
        await _viewModel.NavigateToDestinationCommand.ExecuteAsync(null);
    }

    // Secuencia: logo (fade + scale-up a la vez) -> nombre (fade, con un pequeño delay para que se
    // note como "secuencia" y no todo a la vez) -> eslogan (fade, mismo criterio) -> una pausa
    // corta para que el usuario registre la pantalla antes de navegar. Duración total ~1.82s,
    // dentro del rango de 1.5-2.5s pedido. El splash nativo ya no dibuja logo (blank_splash.svg),
    // así que esta es la única vez que el usuario lo ve aparecer -- sin riesgo de duplicado.
    private async Task PlayEntranceAnimationAsync()
    {
        await Task.WhenAll(
            LogoImage.FadeTo(1, 450, Easing.CubicOut),
            LogoImage.ScaleTo(1, 450, Easing.CubicOut));

        await Task.Delay(150);
        await AppNameLabel.FadeTo(1, 350, Easing.CubicOut);

        await Task.Delay(120);
        await TaglineLabel.FadeTo(1, 350, Easing.CubicOut);

        await Task.Delay(400);
    }
}
