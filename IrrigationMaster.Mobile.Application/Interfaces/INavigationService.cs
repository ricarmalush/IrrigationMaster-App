namespace IrrigationMaster.Mobile.Application.Interfaces;

// Abstrae la navegación de Shell para que los ViewModels sean testeables
// sin necesitar una app MAUI corriendo.
public interface INavigationService
{
    Task GoToAsync(string route);
}
