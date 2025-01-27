using Automation.Interfaces;
using Microsoft.Reactive.Testing;
using NetDaemon.HassModel;
using NSubstitute;

namespace TestAutomation.Helpers;

public class AppTestContext
{
    public HaContextMock HaContextMock { get; } = new();
    public IHaContext HaContext => HaContextMock.HaContext;
    public TestScheduler Scheduler { get; } =  new ();
    public INotify Notify { get; } = Substitute.For<INotify>();

    private AppTestContext()
    {
        Scheduler.AdvanceTo(DateTimeOffset.Now.ToUnixTimeMilliseconds());
    }
    
    public static AppTestContext New()
    {
        return new AppTestContext();
    }
    
    public void AdvanceTimeTo(long absoluteTime)
    {
        Scheduler.AdvanceTo(absoluteTime);
    }
    
    public void AdvanceTimeBy(long absoluteTime)
    {
        Scheduler.AdvanceBy(absoluteTime);
    }
    
    public void SetCurrentTime(DateTime time)
    {
        AdvanceTimeTo(time.Ticks);
    }
}