namespace ModForge;
public static class MauiProgram {
  public static MauiApp CreateMauiApp() => MauiApp.CreateBuilder().UseMauiApp<App>().Build();
}
public class App : Application {
  protected override Window CreateWindow(IActivationState s) => new Window(new MainPage());
}
