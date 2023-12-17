using Avalonia;
using AvaloniaFirstApp.Views.ModalWindows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;

namespace AvaloniaFirstApp;

internal class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        try
        {
            BuildAvaloniaApp()
                .StartWithClassicDesktopLifetime(args);
        }
        catch (Exception ex)
        {
            HandleAllExceptions(ex);
        }
    }

    private static void HandleAllExceptions(Exception ex)
    {
        ExceptionHandlingWindow exWindow = new(ex);
        exWindow.Show(App.MainWindow!);
        throw ex;
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();

    public static IHostBuilder CreateHostBuilder(string[] args)
        => Host.CreateDefaultBuilder(args)
            .UseContentRoot(Environment.CurrentDirectory)
            .ConfigureAppConfiguration(cfg =>
            {
                cfg.SetBasePath(Environment.CurrentDirectory);
                cfg.AddUserSecrets<Program>();
            })
            .ConfigureServices(App.ConfigureServices);
}
