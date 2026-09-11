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
        {
            // Esta página está pensada para verse una sola vez. Si por lo que sea (navegación
            // "atrás", Hot Reload...) vuelve a aparecer, NO debe quedarse congelada: no repetimos
            // la animación, pero sí navegamos de inmediato al destino de siempre.
            await _viewModel.NavigateToDestinationCommand.ExecuteAsync(null);
            return;
        }
        _hasAnimated = true;

        await PlayEntranceAnimationAsync();
        await _viewModel.NavigateToDestinationCommand.ExecuteAsync(null);
    }

    // Secuencia: logo (fade + scale-up a la vez, salvo en WinUI -- ver comentario en LogoImage
    // dentro de PlayEntranceAnimationAsync) -> nombre (fade, con un pequeño delay para que se note
    // como "secuencia" y no todo a la vez) -> eslogan (fade, mismo criterio) -> una pausa corta
    // para que el usuario registre la pantalla antes de navegar. Duración total ~1.82s (~1.37s en
    // WinUI, sin la animación del logo), dentro del rango de 1.5-2.5s pedido. El splash nativo ya
    // no dibuja logo (blank_splash.svg), así que esta es la única vez que el usuario lo ve
    // aparecer, sin riesgo de duplicado.
    private async Task PlayEntranceAnimationAsync()
    {
        // WinUI: animar Opacity/Scale de una Image cuya fuente es un SVG puede quedarse "colgado"
        // en el estado inicial (el logo nunca llega a verse) -- no reproducido en Android. En vez
        // de perseguir un timing más a ciegas en una plataforma que no puedo probar en directo, en
        // Windows el logo se muestra directamente visible, sin animar; en el resto sí se anima.
        if (DeviceInfo.Platform == DevicePlatform.WinUI)
        {
            LogoImage.Opacity = 1;
            LogoImage.Scale = 1;
        }
        else
        {
            await Task.WhenAll(
                LogoImage.FadeTo(1, 450, Easing.CubicOut),
                LogoImage.ScaleTo(1, 450, Easing.CubicOut));
        }

        await Task.Delay(150);
        await AppNameLabel.FadeTo(1, 350, Easing.CubicOut);

        await Task.Delay(120);
        await TaglineLabel.FadeTo(1, 350, Easing.CubicOut);

        await Task.Delay(400);
    }
}
