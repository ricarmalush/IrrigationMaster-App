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

public partial class SystemSettingsViewModel : BindableObject
{
    private readonly IStructureService _structureService;
    private bool _isLoading;

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
    public string WalkwayCode { get => _walkwayCode; set { _walkwayCode = value; OnPropertyChanged(); } }
    public string WalkwayLength { get => _walkwayLength; set { _walkwayLength = value; OnPropertyChanged(); } }

    // --- ESTADOS ---
    public bool IsLoading { get => _isLoading; set { _isLoading = value; OnPropertyChanged(); OnPropertyChanged(nameof(IsNotLoading)); } }
    public bool IsNotLoading => !IsLoading;

    // --- COMANDOS ---
    public ICommand SaveOrganizationCommand { get; }
    public ICommand SaveHydraulicSectorCommand { get; }
    public ICommand SaveWalkwayCommand { get; }
    public ICommand LoadCountriesCommand { get; }

    public SystemSettingsViewModel(IStructureService structureService)
    {
        _structureService = structureService;

        SaveOrganizationCommand = new Command(async () => await ExecuteSaveOrganizationAsync());
        SaveHydraulicSectorCommand = new Command(async () => await ExecuteSaveHydraulicSectorAsync());
        SaveWalkwayCommand = new Command(async () => await ExecuteSaveWalkwayAsync());
        LoadCountriesCommand = new Command(async () => await LoadCountriesAsync());

        // Cargamos los países al instanciar el ViewModel
        LoadCountriesCommand.Execute(null);
    }

    private async Task LoadCountriesAsync()
    {
        try
        {
            // 1. Llamada real a la infraestructura (tu API)
            var countriesFromApi = await _structureService.GetCountriesAsync();

            // 2. Limpiamos la lista actual por si se vuelve a cargar la pantalla
            Countries.Clear();

            // 3. Verificamos que llegaron datos y llenamos el Desplegable (Picker)
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

            // Opcional: Avisar al usuario si falla la carga del catálogo
            // await Shell.Current.DisplayAlert(AppStrings.ErrorTitle, AppStrings.ApiConnectionError, "OK");
        }
    }

    private async Task ExecuteSaveOrganizationAsync()
    {
        if (string.IsNullOrWhiteSpace(OrgName) || string.IsNullOrWhiteSpace(OrgTaxId) ||
            string.IsNullOrWhiteSpace(OrgStreet) || string.IsNullOrWhiteSpace(OrgCity) ||
            string.IsNullOrWhiteSpace(OrgStateOrProvince) || string.IsNullOrWhiteSpace(OrgPostalCode) ||
            SelectedCountry == null)
        {
            // Uso de recursos centralizados
            await Shell.Current.DisplayAlert(AppStrings.AttentionTitle, AppStrings.MsgMissingOrgData, "OK");
            return;
        }

        IsLoading = true;
        try
        {
            var orgPayload = new
            {
                name = OrgName.Trim(),
                taxId = OrgTaxId.Trim(),
                address = new
                {
                    mainAddress = OrgStreet.Trim(),
                    city = OrgCity.Trim(),
                    stateOrProvince = OrgStateOrProvince.Trim(),
                    postalCode = OrgPostalCode.Trim(),
                    countryId = SelectedCountry.Id.ToString(), // ID dinámico del Picker
                    locationDetail = OrgLocationDetail?.Trim() ?? string.Empty
                }
            };

            // TODO: Enviar a tu API (Ej: await _structureService.CreateOrganizationAsync(orgPayload))
            await Task.Delay(800);

            await Shell.Current.DisplayAlert(AppStrings.SuccessTitle, string.Format(AppStrings.OrgCreatedSuccess, OrgName), "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Org]: {ex.Message}");
            await Shell.Current.DisplayAlert(AppStrings.ErrorTitle, AppStrings.ApiConnectionError, "OK");
        }
        finally { IsLoading = false; }
    }

    private async Task ExecuteSaveHydraulicSectorAsync()
    {
        if (string.IsNullOrWhiteSpace(SectorName) || string.IsNullOrWhiteSpace(SectorAreaSize))
        {
            await Shell.Current.DisplayAlert(AppStrings.AttentionTitle, AppStrings.MsgMissingSectorData, "OK");
            return;
        }

        IsLoading = true;
        try
        {
            double.TryParse(SectorAreaSize, out double area);
            // TODO: Enviar a tu API (Ej: await _structureService.CreateHydraulicSectorAsync(sectorPayload))
            await Task.Delay(800);
            await Shell.Current.DisplayAlert(AppStrings.SuccessTitle, string.Format(AppStrings.SectorCreatedSuccess, SectorName), "OK");
            SectorName = string.Empty; SectorAreaSize = string.Empty;
        }
        catch (Exception) { await Shell.Current.DisplayAlert(AppStrings.ErrorTitle, AppStrings.ApiConnectionError, "OK"); }
        finally { IsLoading = false; }
    }

    private async Task ExecuteSaveWalkwayAsync()
    {
        if (string.IsNullOrWhiteSpace(WalkwayCode) || string.IsNullOrWhiteSpace(WalkwayLength))
        {
            await Shell.Current.DisplayAlert(AppStrings.AttentionTitle, AppStrings.MsgMissingWalkwayData, "OK");
            return;
        }

        IsLoading = true;
        try
        {
            double.TryParse(WalkwayLength, out double length);
            // TODO: Enviar a tu API (Ej: await _structureService.CreateWalkwayAsync(walkwayPayload))
            await Task.Delay(800);
            await Shell.Current.DisplayAlert(AppStrings.SuccessTitle, string.Format(AppStrings.WalkwayCreatedSuccess, WalkwayCode), "OK");
            WalkwayCode = string.Empty; WalkwayLength = string.Empty;
        }
        catch (Exception) { await Shell.Current.DisplayAlert(AppStrings.ErrorTitle, AppStrings.ApiConnectionError, "OK"); }
        finally { IsLoading = false; }
    }
}