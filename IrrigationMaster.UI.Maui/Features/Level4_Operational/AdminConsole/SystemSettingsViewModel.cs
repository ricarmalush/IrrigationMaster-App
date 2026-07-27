using IrrigationMaster.Mobile.Application.Common.Dtos;
using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.Application.Interfaces;
using IrrigationMaster.UI.Maui.Common;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

// INotifyPropertyChanged en vez de BindableObject: XAML se enlaza igual (no usa
// BindableProperty), pero así el ViewModel se puede instanciar en tests sin una
// app MAUI corriendo (BindableObject exige un Dispatcher de WinUI en su constructor).
public partial class SystemSettingsViewModel : INotifyPropertyChanged
{
    private readonly IStructureService _structureService;
    private readonly IAlertService _alertService;
    private bool _isLoading;

    public event PropertyChangedEventHandler? PropertyChanged;

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    // --- PESTAÑA 1: ENTIDAD RAÍZ ---
    private string _orgName = string.Empty;
    private string _orgTaxId = string.Empty;
    private string _orgStreet = string.Empty;
    private string _orgCity = string.Empty;
    private string _orgStateOrProvince = string.Empty;
    private string _orgPostalCode = string.Empty;
    private string _orgLocationDetail = string.Empty;
    private CountryItem? _selectedCountry;

    public string OrgName { get => _orgName; set { _orgName = value; OnPropertyChanged(); } }
    public string OrgTaxId { get => _orgTaxId; set { _orgTaxId = value; OnPropertyChanged(); } }
    public string OrgStreet { get => _orgStreet; set { _orgStreet = value; OnPropertyChanged(); } }
    public string OrgCity { get => _orgCity; set { _orgCity = value; OnPropertyChanged(); } }
    public string OrgStateOrProvince { get => _orgStateOrProvince; set { _orgStateOrProvince = value; OnPropertyChanged(); } }
    public string OrgPostalCode { get => _orgPostalCode; set { _orgPostalCode = value; OnPropertyChanged(); } }
    public string OrgLocationDetail { get => _orgLocationDetail; set { _orgLocationDetail = value; OnPropertyChanged(); } }

    // Colección para alimentar el Picker de XAML
    public ObservableCollection<CountryItem> Countries { get; } = [];

    public CountryItem? SelectedCountry
    {
        get => _selectedCountry;
        set { _selectedCountry = value; OnPropertyChanged(); }
    }

    // --- PESTAÑA 2: SECTORES HIDRÁULICOS ---
    private string _sectorName = string.Empty;
    private string _sectorAreaSize = string.Empty;
    public string SectorName { get => _sectorName; set { _sectorName = value; OnPropertyChanged(); } }
    public string SectorAreaSize { get => _sectorAreaSize; set { _sectorAreaSize = value; OnPropertyChanged(); } }

    // --- PESTAÑA 3: ANDADORES ---
    private string _walkwayCode = string.Empty;
    private string _walkwayLength = string.Empty;
    private HydraulicSectorItem? _selectedHydraulicSector;
    public string WalkwayCode { get => _walkwayCode; set { _walkwayCode = value; OnPropertyChanged(); } }
    public string WalkwayLength { get => _walkwayLength; set { _walkwayLength = value; OnPropertyChanged(); } }

    // Colección para alimentar el Picker de XAML
    public ObservableCollection<HydraulicSectorItem> HydraulicSectors { get; } = [];

    public HydraulicSectorItem? SelectedHydraulicSector
    {
        get => _selectedHydraulicSector;
        set { _selectedHydraulicSector = value; OnPropertyChanged(); }
    }

    // --- ESTADOS ---
    public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotLoading)); } }
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
