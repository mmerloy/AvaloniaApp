using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaFirstApp.Infrastructure.Services;
using AvaloniaFirstApp.ViewModels;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Linq;
using System.Reflection;

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
                db.EnsureCreated();
            }

            MainWindow = new MainWindow()
            {
                DataContext = Host.Services.GetRequiredService<MainWindowViewModel>()
            };
            desktop.MainWindow = MainWindow;
        }

        base.OnFrameworkInitializationCompleted();
    }

    internal static void ConfigureServices(HostBuilderContext context, IServiceCollection collection)
    {
        collection.AddDbContext<ApplicationDbContext>(
            cfg =>
            {
                cfg.UseSqlite(context.Configuration.GetConnectionString("SQLite"),
                    lc => lc.MigrationsAssembly("DAL.SQLight")
                );
            }, ServiceLifetime.Singleton);

        collection.AddAutoMapper(
            cfg => cfg.AddProfile<AutoMapper.ModelsProfile>()
        );

        collection.AddSingleton<MainWindowViewModel>();

        collection.AddSingleton<MethodConfigurationViewModelsLocator>();
    }
}