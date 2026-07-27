using CommunityToolkit.Mvvm.ComponentModel;
using IrrigationMaster.Mobile.Application.Common.Dtos;
using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.UI.Maui.Common;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace IrrigationMaster.UI.Maui.Features.Level4_Operational.AdminConsole;

// Clase auxiliar para el desplegable de países
public class CountryItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// Clase auxiliar para el desplegable de sectores hidráulicos
public class HydraulicSectorItem
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

// ObservableObject (CommunityToolkit.Mvvm) en vez de BindableObject: XAML se enlaza igual,
// pero así el ViewModel se puede instanciar en tests sin una app MAUI corriendo
// (BindableObject exige un Dispatcher de WinUI en su constructor). Mismo patrón que
// RegisterViewModel, ahora estándar para toda la App.
public partial class SystemSettingsViewModel : ObservableObject
{
    private readonly IStructureService _structureService;
    private readonly IAlertService _alertService;

    // --- PESTAÑA 1: ENTIDAD RAÍZ ---
    [ObservableProperty] public partial string OrgName { get; set; } = string.Empty;
    [ObservableProperty] public partial string OrgTaxId { get; set; } = string.Empty;
    [ObservableProperty] public partial string OrgStreet { get; set; } = string.Empty;
    [ObservableProperty] public partial string OrgCity { get; set; } = string.Empty;
    [ObservableProperty] public partial string OrgStateOrProvince { get; set; } = string.Empty;
    [ObservableProperty] public partial string OrgPostalCode { get; set; } = string.Empty;
    [ObservableProperty] public partial string OrgLocationDetail { get; set; } = string.Empty;
    [ObservableProperty] public partial CountryItem? SelectedCountry { get; set; }

    // Colección para alimentar el Picker de XAML
    public ObservableCollection<CountryItem> Countries { get; } = [];

    // --- PESTAÑA 2: SECTORES HIDRÁULICOS ---
    [ObservableProperty] public partial string SectorName { get; set; } = string.Empty;
    [ObservableProperty] public partial string SectorAreaSize { get; set; } = string.Empty;

    // --- PESTAÑA 3: ANDADORES ---
    [ObservableProperty] public partial string WalkwayCode { get; set; } = string.Empty;
    [ObservableProperty] public partial string WalkwayLength { get; set; } = string.Empty;
    [ObservableProperty] public partial HydraulicSectorItem? SelectedHydraulicSector { get; set; }

    // Colección para alimentar el Picker de XAML
    public ObservableCollection<HydraulicSectorItem> HydraulicSectors { get; } = [];

    // --- ESTADOS ---
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    public partial bool IsLoading { get; set; }

    public bool IsNotLoading => !IsLoading;

    // --- COMANDOS ---
    public ICommand SaveOrganizationCommand { get; }
    public ICommand SaveHydraulicSectorCommand { get; }
    public ICommand SaveWalkwayCommand { get; }
    public ICommand LoadCountriesCommand { get; }
    public ICommand LoadHydraulicSectorsCommand { get; }

    public SystemSettingsViewModel(IStructureService structureService, IAlertService alertService)
    {
        _structureService = structureService;
        _alertService = alertService;

        SaveOrganizationCommand = new Command(async () => await ExecuteSaveOrganizationAsync());
        SaveHydraulicSectorCommand = new Command(async () => await ExecuteSaveHydraulicSectorAsync());
        SaveWalkwayCommand = new Command(async () => await ExecuteSaveWalkwayAsync());
        LoadCountriesCommand = new Command(async () => await LoadCountriesAsync());
        LoadHydraulicSectorsCommand = new Command(async () => await LoadHydraulicSectorsAsync());

        // Cargamos los catálogos al instanciar el ViewModel
        LoadCountriesCommand.Execute(null);
        LoadHydraulicSectorsCommand.Execute(null);
    }

    private async Task LoadCountriesAsync()
    {
        try
        {
            var countriesFromApi = await _structureService.GetCountriesAsync();

            Countries.Clear();

            if (countriesFromApi != null)
            {
                foreach (var country in countriesFromApi)
                {
                    Countries.Add(new CountryItem
                    {
                        Id = country.Id,
                        Name = country.Name
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Loading Countries]: {ex.Message}");
        }
    }

    private async Task LoadHydraulicSectorsAsync()
    {
        try
        {
            var sectorsFromApi = await _structureService.GetHydraulicSectorsAsync();

            HydraulicSectors.Clear();

            if (sectorsFromApi != null)
            {
                foreach (var sector in sectorsFromApi)
                {
                    HydraulicSectors.Add(new HydraulicSectorItem
                    {
                        Id = sector.Id,
                        Name = sector.Name
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Loading HydraulicSectors]: {ex.Message}");
        }
    }

    internal async Task ExecuteSaveOrganizationAsync()
    {
        if (string.IsNullOrWhiteSpace(OrgName) || string.IsNullOrWhiteSpace(OrgTaxId) ||
            string.IsNullOrWhiteSpace(OrgStreet) || string.IsNullOrWhiteSpace(OrgCity) ||
            string.IsNullOrWhiteSpace(OrgStateOrProvince) || string.IsNullOrWhiteSpace(OrgPostalCode) ||
            SelectedCountry == null)
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgMissingOrgData);
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _structureService.CreateOrganizationAsync(new CreateOrganizationRequest
            {
                Name = OrgName.Trim(),
                TaxId = OrgTaxId.Trim(),
                Address = new AddressRequest
                {
                    MainAddress = OrgStreet.Trim(),
                    City = OrgCity.Trim(),
                    StateOrProvince = OrgStateOrProvince.Trim(),
                    PostalCode = OrgPostalCode.Trim(),
                    CountryId = SelectedCountry.Id, // ID dinámico del Picker
                    LocationDetail = OrgLocationDetail?.Trim()
                }
            });

            if (result.IsSuccess)
            {
                await _alertService.ShowAsync(AppStrings.SuccessTitle, string.Format(AppStrings.OrgCreatedSuccess, OrgName));
            }
            else
            {
                await _alertService.ShowAsync(AppStrings.ErrorTitle, BuildFailureMessage(result));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Org]: {ex.Message}");
            await _alertService.ShowAsync(AppStrings.ErrorTitle, AppStrings.ApiConnectionError);
        }
        finally { IsLoading = false; }
    }

    internal async Task ExecuteSaveHydraulicSectorAsync()
    {
        if (string.IsNullOrWhiteSpace(SectorName) || !decimal.TryParse(SectorAreaSize, out var area))
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgMissingSectorData);
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _structureService.CreateHydraulicSectorAsync(new CreateHydraulicSectorRequest
            {
                Name = SectorName.Trim(),
                AreaSize = area
            });

            if (result.IsSuccess)
            {
                await _alertService.ShowAsync(AppStrings.SuccessTitle, string.Format(AppStrings.SectorCreatedSuccess, SectorName));
                SectorName = string.Empty;
                SectorAreaSize = string.Empty;

                // El sector recién creado debe poder elegirse ya en la pestaña de Andadores
                await LoadHydraulicSectorsAsync();
            }
            else
            {
                await _alertService.ShowAsync(AppStrings.ErrorTitle, BuildFailureMessage(result));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Sector]: {ex.Message}");
            await _alertService.ShowAsync(AppStrings.ErrorTitle, AppStrings.ApiConnectionError);
        }
        finally { IsLoading = false; }
    }

    internal async Task ExecuteSaveWalkwayAsync()
    {
        if (string.IsNullOrWhiteSpace(WalkwayCode) || !decimal.TryParse(WalkwayLength, out var length) ||
            SelectedHydraulicSector == null)
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgMissingWalkwayData);
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _structureService.CreateWalkwayAsync(new CreateWalkwayRequest
            {
                Code = WalkwayCode.Trim(),
                Length = length,
                HydraulicSectorId = SelectedHydraulicSector.Id
            });

            if (result.IsSuccess)
            {
                await _alertService.ShowAsync(AppStrings.SuccessTitle, string.Format(AppStrings.WalkwayCreatedSuccess, WalkwayCode));
                WalkwayCode = string.Empty;
                WalkwayLength = string.Empty;
            }
            else
            {
                await _alertService.ShowAsync(AppStrings.ErrorTitle, BuildFailureMessage(result));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Walkway]: {ex.Message}");
            await _alertService.ShowAsync(AppStrings.ErrorTitle, AppStrings.ApiConnectionError);
        }
        finally { IsLoading = false; }
    }

    private static string BuildFailureMessage(StructureOperationResult result)
    {
        if (result.Errors is { Count: > 0 })
            return string.Join("\n", result.Errors.Select(e => $"{e.PropertyMessage}: {e.ErrorMessage}"));

        return string.IsNullOrWhiteSpace(result.Message) ? AppStrings.ApiConnectionError : result.Message;
    }
}
