

using XRobot.Views;
using XRobot.Services;

using SkiaSharp.Views.Maui.Controls.Hosting;
using XRobot.ViewModels;

namespace XRobot;
public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .UseSkiaSharp()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OriginTech-Regular.ttf", "OriginTechRegular");
            });

        var services = builder.Services;

        services.AddTransient<IAlertMessage, AlertMessage>();

        services.AddTransient<BluetoothController>();

        services.AddTransient<App>();

        services.AddTransient<MainView>();
        services.AddTransient<MainViewModel>();


        return builder.Build();
    }
}
