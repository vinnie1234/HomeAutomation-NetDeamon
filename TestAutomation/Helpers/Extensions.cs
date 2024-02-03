using HomeAssistantGenerated;
using Microsoft.Extensions.DependencyInjection;

namespace TestAutomation.Helpers;

public static class Extensions
{
    public static IServiceCollection AddGeneratedCode(this IServiceCollection serviceCollection)
        => serviceCollection
            .AddTransient<Entities>()
            .AddTransient<Services>();    
}