using Android.App;
using Android.Content;
using Android.Content.PM;
using Android.OS;
using Firebase;
using Plugin.Firebase.CloudMessaging;
using Plugin.Firebase.Core.Platforms.Android;

namespace IrrigationMaster.UI.Maui
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        // Debe coincidir con el ChannelId que asigna FirebaseCloudMessagingImplementation.ChannelId
        // más abajo -- es el canal en el que el sistema muestra las notificaciones locales que la
        // propia librería genera cuando la App está en segundo plano.
        private const string NotificationChannelId = "irrigationmaster.general";

        protected override void OnCreate(Bundle? savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            // FirebaseOptions.FromResource(this) lee los recursos que genera en build-time el
            // target ProcessGoogleServicesJson (ver GoogleServicesJson en el .csproj) a partir de
            // google-services.json -- es el único método público de FirebaseOptions en este
            // binding (verificado por reflexión, sin Builder ni constructor público).
            CrossFirebase.Initialize(this, () => this, FirebaseOptions.FromResource(this), "irrigationmaster");

            CreateNotificationChannel();

            // Requerido por Plugin.Firebase.CloudMessaging (ver cloud_messaging.md del paquete):
            // sin esto, tocar una notificación con la App cerrada/en segundo plano no dispara
            // NotificationTapped.
            if (Intent is not null)
                FirebaseCloudMessagingImplementation.OnNewIntent(Intent);

            RequestPostNotificationsPermissionIfNeeded();
        }

        protected override void OnNewIntent(Intent? intent)
        {
            base.OnNewIntent(intent);

            if (intent is not null)
                FirebaseCloudMessagingImplementation.OnNewIntent(intent);
        }

        private void CreateNotificationChannel()
        {
            var notificationManager = (NotificationManager)GetSystemService(NotificationService)!;
            var channel = new NotificationChannel(NotificationChannelId, "General", NotificationImportance.Default);
            notificationManager.CreateNotificationChannel(channel);

            FirebaseCloudMessagingImplementation.ChannelId = NotificationChannelId;
        }

        // Android 13+ (API 33) exige este permiso en tiempo de ejecución, no basta con declararlo
        // en el manifiesto -- sin él, ninguna notificación local (incluidas las push en segundo
        // plano) se muestra.
        private void RequestPostNotificationsPermissionIfNeeded()
        {
            if (Build.VERSION.SdkInt < BuildVersionCodes.Tiramisu) return;

            if (CheckSelfPermission(Android.Manifest.Permission.PostNotifications) != Permission.Granted)
                RequestPermissions([Android.Manifest.Permission.PostNotifications], requestCode: 0);
        }
    }
}
