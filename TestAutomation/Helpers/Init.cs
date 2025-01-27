using Automation.apps;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace TestAutomation.Helpers;

public static class Init
{
    public static T InitApp<T>(this AppTestContext ctx) where T : BaseApp
    {
        var logger = Substitute.For<ILogger<T>>();
        return (T)Activator.CreateInstance(typeof(T), ctx.HaContext, logger, ctx.Notify, ctx.Scheduler)!;
    }
}