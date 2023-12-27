using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaFirstApp.Infrastructure.Services;
using AvaloniaFirstApp.Infrastructure.Services.Notifications;
using AvaloniaFirstApp.Infrastructure.Services.Prediction;
using AvaloniaFirstApp.ViewModels;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace AvaloniaFirstApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static Window? MainWindow { get; private set; }

    private static IHost? _host;

    public static IHost Host => _host ??= Program.CreateHostBuilder(Environment.GetCommandLineArgs()).Build();

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            using (var scope = Host.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>().Database;
                //db.EnsureCreated();
                db.Migrate();
            }

            MainWindow = new MainWindow()
            {
                DataContext = Host.Services.GetRequiredService<MainWindowViewModel>()
            };
            desktop.MainWindow = MainWindow;
            desktop.Exit += DisableHost;
        }
        base.OnFrameworkInitializationCompleted();
    }

    private async void DisableHost(object? sender, ControlledApplicationLifetimeExitEventArgs e)
    {
        if (Host is null)
            return;
        
        await Host.StopAsync();
        Host.Dispose();
    }

    internal static void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
    {
        collection.AddDbContext<ApplicationDbContext>(
            cfg =>
            {
                cfg.UseSqlite($"Data Source=\"{context.Configuration["SQLitePath"]}\"",
                    lc => lc.MigrationsAssembly("DAL.SQLight")
                );
            }, ServiceLifetime.Singleton);

        collection.AddAutoMapper(
            cfg => cfg.AddProfile<AutoMapper.ModelsProfile>()
        );

        //collection.AddSingleton<IPredictionService>(s => new PredictionServiceStub(123));
        collection.AddSingleton<IPredictionService>(s => new PythonScriptPredictionService(
            context.Configuration["pyScript"]!,
            context.Configuration["tchModelPath"]!,
            context.Configuration["outputImagesDirectoryPath"]!)
        );
        collection.AddSingleton<MainWindowViewModel>();

        collection.AddSingleton<INotifier, MessageBoxNotifier>();

        collection.AddSingleton<MethodConfigurationViewModelsLocator>();
    }
}