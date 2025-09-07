using Mekatrol.CAM.Core.Parsers.Svg;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NLog.Extensions.Logging;
using System;
using System.IO;
using System.Reflection;

namespace MekatrolCAM;

public class ApplicationServices
{
    public static ApplicationServices Instance { get; } = new ApplicationServices();

    private readonly IServiceProvider serviceProvider;
    private readonly ILogger _logger;

    public ApplicationServices()
    {
        var exeDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty;

        // Build a config object, using env vars and JSON providers.
        var configBuilder = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile(Path.Combine(exeDirectory, "appsettings.json"))
            .AddJsonFile(Path.Combine(exeDirectory, $"appsettings.Development.json"), true, false)
            .AddEnvironmentVariables();

        // Build the config from json settings
        var configuration = configBuilder.Build();

        var services = new ServiceCollection();

        AddServices(services);

        // Configure logging for this method (prior to real one being built).
        var loggingSection = configuration.GetSection("logging");
        serviceProvider = ConfigureLogging(services, loggingSection);

        var logFactory = serviceProvider.GetRequiredService<ILoggerFactory>();
        _logger = logFactory.CreateLogger("app");
    }

    private static void AddServices(ServiceCollection services)
    {
        services.AddSingleton<ISvgParser, SvgParser>();
    }

    public T GetRequiredService<T>() where T : notnull
    {
        return serviceProvider.GetRequiredService<T>();
    }

    public T? GetService<T>()
    {
        return serviceProvider.GetService<T>();
    }

    public ILogger GetLogger()
    {
        return _logger;
    }

    private static IServiceProvider ConfigureLogging(ServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(loggingBuilder =>
        {
            // Get the NLog config section
            var nlogSection = configuration.GetSection("NLog");

            // Set NLog configuration
            NLog.LogManager.Configuration = new NLogLoggingConfiguration(nlogSection);

            // Configure .NET logging
            loggingBuilder
                .AddConfiguration(configuration)
                .AddNLog();
        });

        var serviceProvider = services.BuildServiceProvider();
        return serviceProvider;
    }
}
