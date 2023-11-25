using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using AvaloniaFirstApp.ViewModels;
using Infrastructure.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Reflection;

namespace AvaloniaFirstApp;

public partial class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public static Window? MainWindow { get; private set; }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            MainWindow = new MainWindow()
            {
                DataContext = new MainWindowViewModel()
                //{
                //    MyDoubleValue = 119293.111,
                //    SomeText = "Hello!",
                //}
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
            });
    }
}