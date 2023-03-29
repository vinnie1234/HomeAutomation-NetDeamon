using System.IO;
using Automation.apps;
using Automation.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using NetDaemon.Common.Configuration;

namespace Automation;

internal static class AppServicesExtension
{
    public static IHostBuilder AddAppServices(this IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((_, services) =>
        {
            services.AddSingleton<IDataRepository>(n => new DataRepository(
                    Path.Combine(
                        n.GetRequiredService<IOptions<NetDaemonSettings>>().Value.GetAppSourceDirectory()
                        , ".storage")))
                .AddSingleton<INotify>(x => new Notify(GenericHelpers.GetHaContext(x)));
        });
    }
}