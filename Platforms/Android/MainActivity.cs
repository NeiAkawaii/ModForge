using Android.App; using Android.Content.PM;
namespace ModForge;
[Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity { }
[Application]
public class MainApplication : MauiApplication {
  public MainApplication(IntPtr h, Android.Runtime.JniHandleOwnership o) : base(h, o) { }
  protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();
}
