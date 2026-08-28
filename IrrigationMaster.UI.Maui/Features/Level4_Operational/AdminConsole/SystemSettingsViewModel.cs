using CommunityToolkit.Mvvm.ComponentModel;
using IrrigationMaster.Mobile.Application.Common.Dtos;
using IrrigationMaster.Mobile.Application.Features.Models.Structure;
using IrrigationMaster.Mobile.Application.Features.Models.Users;
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

// SystemSettingsPage ya no es un TabbedPage (ver ApplyTabVisibility para el motivo): esto
// identifica cuál de las Views de contenido está activa dentro de su propio Grid.
public enum SettingsTab
{
    Entidad,
    Sectores,
    Andadores,
    MiCuenta
}

// ObservableObject (CommunityToolkit.Mvvm) en vez de BindableObject: XAML se enlaza igual,
// pero así el ViewModel se puede instanciar en tests sin una app MAUI corriendo
// (BindableObject exige un Dispatcher de WinUI en su constructor). Mismo patrón que
// RegisterViewModel, ahora estándar para toda la App.
public partial class SystemSettingsViewModel : ObservableObject
{
    private readonly IStructureService _structureService;
    private readonly IAuthService _authService;
    private readonly IAlertService _alertService;
    private readonly ICurrentSession _currentSession;

    // --- MI ORGANIZACIÓN (solo lectura, de la organización del usuario logueado) ---
    [ObservableProperty] public partial string MyOrganizationName { get; set; } = string.Empty;
    [ObservableProperty] public partial string MyOrganizationInvitationCode { get; set; } = string.Empty;

    // Un Vecino no debe poder ver ni compartir libremente el código de invitación de su
    // organización -- a quién se lo comparte lo decide el Presidente. Vive dentro de la
    // pestaña "Mi Cuenta" (que sí es visible para los 3 roles), oculta solo esta sección.
    [ObservableProperty] public partial bool ShowMyOrganization { get; set; }

    // El backend ya rechaza con 403 la creación de Organización/Sector/Andador si el llamador
    // no tiene permiso -- esto solo evita mostrarle a cada rol pestañas que nunca van a
    // funcionarle. Se calculan de forma SÍNCRONA en el constructor (a partir de
    // ICurrentSession.CachedRole, ya disponible en memoria tras el login) para que
    // SystemSettingsPage pueda decidir sus Children ANTES del primer render.
    // Vecino no tiene ninguna de las dos en true: solo ve "Mi Cuenta".
    [ObservableProperty] public partial bool ShowEntidadTab { get; set; }
    [ObservableProperty] public partial bool ShowSectoresTab { get; set; }
    [ObservableProperty] public partial bool ShowAndadoresTab { get; set; }

    // Pestaña actualmente mostrada. SystemSettingsPage ya no es un TabbedPage nativo de Android
    // (ver el comentario grande en ApplyTabVisibility para el porqué): esto sustituye la
    // selección de pestaña nativa por una decidida aquí, con las 4 Views de contenido
    // presentes en el mismo Grid y solo la activa visible.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEntidadTabActive))]
    [NotifyPropertyChangedFor(nameof(IsSectoresTabActive))]
    [NotifyPropertyChangedFor(nameof(IsAndadoresTabActive))]
    [NotifyPropertyChangedFor(nameof(IsMiCuentaTabActive))]
    public partial SettingsTab ActiveTab { get; set; }

    public bool IsEntidadTabActive => ActiveTab == SettingsTab.Entidad;
    public bool IsSectoresTabActive => ActiveTab == SettingsTab.Sectores;
    public bool IsAndadoresTabActive => ActiveTab == SettingsTab.Andadores;
    public bool IsMiCuentaTabActive => ActiveTab == SettingsTab.MiCuenta;

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

    // --- MI CUENTA (cambio de contraseña, disponible para cualquier rol autenticado) ---
    [ObservableProperty] public partial string CurrentPassword { get; set; } = string.Empty;
    [ObservableProperty] public partial string NewPassword { get; set; } = string.Empty;
    [ObservableProperty] public partial string ConfirmNewPassword { get; set; } = string.Empty;

    // --- ESTADOS ---
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotLoading))]
    public partial bool IsLoading { get; set; }

    public bool IsNotLoading => !IsLoading;

    // --- COMANDOS ---
    public ICommand SaveOrganizationCommand { get; }
    public ICommand SaveHydraulicSectorCommand { get; }
    public ICommand SaveWalkwayCommand { get; }
    public ICommand ChangePasswordCommand { get; }
    public ICommand LoadCountriesCommand { get; }
    public ICommand LoadHydraulicSectorsCommand { get; }
    public ICommand LoadMyOrganizationCommand { get; }
    public ICommand RegenerateInvitationCodeCommand { get; }
    public ICommand SelectEntidadTabCommand { get; }
    public ICommand SelectSectoresTabCommand { get; }
    public ICommand SelectAndadoresTabCommand { get; }
    public ICommand SelectMiCuentaTabCommand { get; }

    private const string SuperAdminRoleCode = "SUPERADMIN";
    private const string PresidenteRoleCode = "PRESIDENTE";
    private const string VicepresidenteRoleCode = "VICEPRESIDENTE";

    public SystemSettingsViewModel(IStructureService structureService, IAuthService authService, IAlertService alertService, ICurrentSession currentSession)
    {
        _structureService = structureService;
        _authService = authService;
        _alertService = alertService;
        _currentSession = currentSession;

        SaveOrganizationCommand = new Command(async () => await ExecuteSaveOrganizationAsync());
        SaveHydraulicSectorCommand = new Command(async () => await ExecuteSaveHydraulicSectorAsync());
        SaveWalkwayCommand = new Command(async () => await ExecuteSaveWalkwayAsync());
        ChangePasswordCommand = new Command(async () => await ExecuteChangePasswordAsync());
        LoadCountriesCommand = new Command(async () => await LoadCountriesAsync());
        LoadHydraulicSectorsCommand = new Command(async () => await LoadHydraulicSectorsAsync());
        LoadMyOrganizationCommand = new Command(async () => await LoadMyOrganizationAsync());
        RegenerateInvitationCodeCommand = new Command(async () => await ExecuteRegenerateInvitationCodeAsync());
        SelectEntidadTabCommand = new Command(() => ActiveTab = SettingsTab.Entidad);
        SelectSectoresTabCommand = new Command(() => ActiveTab = SettingsTab.Sectores);
        SelectAndadoresTabCommand = new Command(() => ActiveTab = SettingsTab.Andadores);
        SelectMiCuentaTabCommand = new Command(() => ActiveTab = SettingsTab.MiCuenta);

        ApplyTabVisibility(currentSession.CachedRole);

        // Cargamos los catálogos y los datos de "Mi Organización" al instanciar el ViewModel
        LoadCountriesCommand.Execute(null);
        LoadHydraulicSectorsCommand.Execute(null);
        LoadMyOrganizationCommand.Execute(null);
    }

    // Síncrono a propósito: SystemSettingsPage lo llama desde su propio constructor, antes de
    // InitializeComponent()/antes de que la página se añada a la pila de navegación, para
    // decidir qué pestañas existen ANTES de su primer render.
    //
    // SystemSettingsPage dejó de ser un TabbedPage nativo: empujar un TabbedPage con
    // Navigation.PushAsync sobre la pila de un NavigationPage/Shell es un bug conocido de
    // Android (el ViewPager/FragmentManager nativo del TabbedPage choca con las transiciones de
    // fragments del NavigationPage que lo aloja -- reproducido en dispositivo real con
    // IllegalArgumentException: 'No view found for id ... navigationlayout_toptabs',
    // independientemente de cómo se gestionaran sus Children). La única forma de evitarlo sin
    // perder la flecha de retroceso nativa (que exige PushAsync, no PushModalAsync) era dejar de
    // usar TabbedPage: ahora es un ContentPage normal -- empujado exactamente igual que
    // UserManagementPage -- con una tira de pestañas hecha a mano y las 4 Views de contenido en
    // el mismo Grid, mostrando solo la que ActiveTab señale.
    internal void ApplyTabVisibility(string? role)
    {
        bool isSuperAdmin = string.Equals(role, SuperAdminRoleCode, StringComparison.OrdinalIgnoreCase);
        bool isPresidente = string.Equals(role, PresidenteRoleCode, StringComparison.OrdinalIgnoreCase);
        bool isVicepresidente = string.Equals(role, VicepresidenteRoleCode, StringComparison.OrdinalIgnoreCase);

        ShowEntidadTab = isSuperAdmin;
        ShowSectoresTab = isSuperAdmin || isPresidente || isVicepresidente;
        ShowAndadoresTab = isSuperAdmin || isPresidente || isVicepresidente;
        ShowMyOrganization = isSuperAdmin || isPresidente || isVicepresidente;

        // La pestaña inicial es la primera que el rol puede ver, en el mismo orden en que se
        // muestran (Entidad, Sectores, Andadores, Mi Cuenta).
        ActiveTab = ShowEntidadTab ? SettingsTab.Entidad
            : ShowSectoresTab ? SettingsTab.Sectores
            : ShowAndadoresTab ? SettingsTab.Andadores
            : SettingsTab.MiCuenta;
    }

    internal async Task LoadMyOrganizationAsync()
    {
        try
        {
            var organizationIdRaw = await _currentSession.GetOrganizationIdAsync();
            if (!Guid.TryParse(organizationIdRaw, out var organizationId))
                return;

            var organization = await _structureService.GetOrganizationAsync(organizationId);
            if (organization != null)
            {
                MyOrganizationName = organization.Name;
                MyOrganizationInvitationCode = organization.InvitationCode;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error Loading MyOrganization]: {ex.Message}");
        }
    }

    // Regenera el código de invitación de la organización del usuario logueado. Visible solo si
    // ShowMyOrganization es true (SUPERADMIN/Presidente/VicePresidente) -- mismo gating que ya
    // protege la sección entera "Mi Organización" donde vive el código, así que no hace falta una
    // propiedad de visibilidad nueva. El backend vuelve a exigir SUPERADMIN o el permiso
    // MANAGE_ORGANIZATION_CODE de todos modos, así que esto no es la única defensa.
    internal async Task ExecuteRegenerateInvitationCodeAsync()
    {
        var confirmed = await _alertService.ShowConfirmAsync(
            AppStrings.AttentionTitle,
            AppStrings.MsgConfirmRegenerateInvitationCode,
            "Regenerar",
            "Cancelar");

        if (!confirmed)
            return;

        var organizationIdRaw = await _currentSession.GetOrganizationIdAsync();
        if (!Guid.TryParse(organizationIdRaw, out var organizationId))
            return;

        IsLoading = true;
        try
        {
            var result = await _structureService.RegenerateInvitationCodeAsync(organizationId);

            if (result.IsSuccess && !string.IsNullOrWhiteSpace(result.Data))
            {
                // Solo se actualiza el código en pantalla si el backend confirmó éxito -- un fallo
                // nunca debe dejar MyOrganizationInvitationCode en un estado a medias ni distinto
                // del que de verdad hay en el backend.
                MyOrganizationInvitationCode = result.Data;
                await _alertService.ShowAsync(AppStrings.SuccessTitle, AppStrings.InvitationCodeRegeneratedSuccess);
            }
            else
            {
                await _alertService.ShowAsync(AppStrings.ErrorTitle, BuildFailureMessage(result));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error RegenerateInvitationCode]: {ex.Message}");
            await _alertService.ShowAsync(AppStrings.ErrorTitle, AppStrings.ApiConnectionError);
        }
        finally
        {
            IsLoading = false;
        }
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

    internal async Task ExecuteChangePasswordAsync()
    {
        if (string.IsNullOrWhiteSpace(CurrentPassword) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmNewPassword))
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgMissingPasswordData);
            return;
        }

        // Validación local, sin red: el backend también la hace (y la mostraríamos igual, tal
        // cual, si llegara desde ahí), pero comprobarlo aquí evita una petición innecesaria y
        // le da al usuario el mismo mensaje al instante.
        if (NewPassword != ConfirmNewPassword)
        {
            await _alertService.ShowAsync(AppStrings.AttentionTitle, AppStrings.MsgPasswordsDoNotMatch);
            return;
        }

        IsLoading = true;
        try
        {
            var result = await _authService.ChangePasswordAsync(CurrentPassword, NewPassword, ConfirmNewPassword);

            if (result.IsSuccess)
            {
                await _alertService.ShowAsync(AppStrings.SuccessTitle, AppStrings.PasswordChangedSuccess);
                CurrentPassword = string.Empty;
                NewPassword = string.Empty;
                ConfirmNewPassword = string.Empty;
            }
            else
            {
                // Mismo criterio que el resto de la pantalla: se muestra tal cual el mensaje que
                // devuelva el backend (p. ej. "La contraseña actual no es correcta."), sin
                // reinterpretarlo aquí.
                await _alertService.ShowAsync(AppStrings.ErrorTitle, BuildFailureMessage(result));
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[Error ChangePassword]: {ex.Message}");
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

    private static string BuildFailureMessage(UserActionResult result)
    {
        if (result.Errors is { Count: > 0 })
            return string.Join("\n", result.Errors.Select(e => $"{e.PropertyMessage}: {e.ErrorMessage}"));

        return string.IsNullOrWhiteSpace(result.Message) ? AppStrings.ApiConnectionError : result.Message;
    }

    private static string BuildFailureMessage(RegenerateInvitationCodeResult result)
    {
        if (result.Errors is { Count: > 0 })
            return string.Join("\n", result.Errors.Select(e => $"{e.PropertyMessage}: {e.ErrorMessage}"));

        return string.IsNullOrWhiteSpace(result.Message) ? AppStrings.ApiConnectionError : result.Message;
    }
}
