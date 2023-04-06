
using NetDaemon.Extensions.Scheduler;
using NetDaemon.HassModel;

namespace TestAutomation.Helpers;

/// <summary>
///     Helper class to handle state of the test session
/// </summary>
public class AppTestContext
{
    public HaContextMock HaContextMock { get; } = new();
    public IHaContext HaContext => HaContextMock.HaContext;
    public INetDaemonScheduler Scheduler { get; }

    public static AppTestContext New()
    {
        return new();
    }
}