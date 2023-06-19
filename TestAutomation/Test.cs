using Automation.apps;
using Automation.apps.General;
using Microsoft.Extensions.Logging;
using NSubstitute;
using TestAutomation.Helpers;

namespace TestAutomation;

public class Test
{
    private readonly AppTestContext _ctx = AppTestContext.New();

    [Fact]
    public void Test1()
    {
        _ctx.InitHouseManagerApp();
    }
}

public static class HouseStateManagerInstanceExtensions
{
    public static HouseStateManager InitHouseManagerApp(this AppTestContext ctx)
    {
        var loggerMock = Substitute.For<ILogger<HouseStateManager>>();
        return new HouseStateManager(ctx.HaContext, loggerMock,
            new Notify(ctx.HaContext, null), ctx.Scheduler);
    }
}