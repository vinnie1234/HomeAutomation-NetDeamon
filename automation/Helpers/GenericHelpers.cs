using Microsoft.Extensions.DependencyInjection;

namespace Automation.Helpers;

public static class GenericHelpers
{
    public static IHaContext GetHaContext(IServiceProvider serviceProvider)
    {
        var scope = serviceProvider.CreateScope();
        var haContext = scope.ServiceProvider.GetService<IHaContext>();
        if (haContext == null)
        {
            throw new Exception("Unable to get correct HaContext");
        }

        return haContext;
    }
}