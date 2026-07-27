using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace IrrigationMaster.UI.Maui.Features.Level1_Core.Register;

public partial class RegisterViewModel : ObservableObject
{
    // =========================================================================
    // PROPIEDADES OBSERVABLES (Estándar C# 13 / .NET 9 compatible con AOT)
    // =========================================================================

    [ObservableProperty] public partial string FirstName { get; set; } = string.Empty;
    [ObservableProperty] public partial string LastName { get; set; } = string.Empty;
    [ObservableProperty] public partial string Email { get; set; } = string.Empty;
    [ObservableProperty] public partial string Password { get; set; } = string.Empty;
    [ObservableProperty] public partial string PlotNumber { get; set; } = string.Empty;
    [ObservableProperty] public partial string SelectedWalkway { get; set; } = string.Empty;
    [ObservableProperty] public partial bool IsBusy { get; set; }

    // =========================================================================
    // COLECCIONES Y MAESTROS
    // =========================================================================

    // Colección observable para rellenar de forma dinámica las zonas/andadores de El Saso
    public ObservableCollection<string> WalkwaysList { get; } =
    [
        "Andador 1", "Andador 2", "Andador 3", "Andador 4",
        "Andador 5", "Andador 6", "Andador 7", "Andador 8", "Andador 9"
    ];

    // =========================================================================
    // COMANDOS DE ACCIÓN (MVVM)
    // =========================================================================

    [RelayCommand]
    private async Task RegisterAsync()
    {
        // Validación limpia usando las propiedades automáticas generadas por el Toolkit
        if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(Email) ||
            string.IsNullOrWhiteSpace(Password) || string.IsNullOrWhiteSpace(SelectedWalkway))
        {
            await Shell.Current.DisplayAlert("Atención", "Por favor, rellena los campos obligatorios.", "OK");
            return;
        }

        IsBusy = true;

        try
        {
            // TODO: Mapear a tu DTO e invocar tu ApiService de forma asíncrona, ej:
            // var result = await _accountApi.RegisterAsync(new RegisterDto(...));

            await Task.Delay(1500); // Simulación de carga de red

            await Shell.Current.DisplayAlert("Registro Exitoso", "Enviando datos a la API de El Saso... Cuenta creada.", "OK");

            // Regresa una pantalla atrás en la navegación
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlert("Error de Registro", ex.Message, "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}