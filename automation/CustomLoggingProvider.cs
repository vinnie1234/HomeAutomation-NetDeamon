using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace Automation;

public static class CustomLoggingProvider
{
    //https://netdaemon.xyz/docs/v2/app_model/app_model_custom_logging/

    /// <summary>
    ///     Adds standard serilog logging configuration, from app settings, as per:
    ///     https://github.com/datalust/dotnet6-serilog-example
    /// </summary>
    /// <param name="builder"></param>
    public static IHostBuilder UseCustomLogging(this IHostBuilder builder)
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(@"appsettings.json")
            .Build();

        var logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        logger.Debug("");

        return builder.UseSerilog(logger);
    }
}