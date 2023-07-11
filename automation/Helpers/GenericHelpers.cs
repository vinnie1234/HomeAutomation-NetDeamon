using Microsoft.Extensions.DependencyInjection;

namespace Automation.Helpers;

public static class GenericHelpers
{
    public static IHaContext GetHaContext(IServiceProvider serviceProvider)
    {
        var scope = serviceProvider.CreateScope();
        var haContext = scope.ServiceProvider.GetRequiredService<IHaContext>();

        return haContext;
    }
}