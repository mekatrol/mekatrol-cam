using Avalonia;
using Avalonia.ReactiveUI;
using MekatrolCAM.ViewModels;
using MekatrolCAM.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;

namespace MekatrolCAM;

sealed class Program
{
    public static IHost AppHost { get; private set; } = null!;

    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddSingleton<MainWindow>();
        builder.Services.AddSingleton<MainWindowViewModel>();
        builder.Services.AddSingleton<IWindowService, WindowService>();

        builder.Logging.AddConsole();

        AppHost = builder.Build();

        var exitCode = BuildAvaloniaApp().StartWithClassicDesktopLifetime(args);

        // Dispose host on exit
        AppHost.Dispose();
        Environment.Exit(exitCode);
    }

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .UseReactiveUI()
            .LogToTrace();
}
