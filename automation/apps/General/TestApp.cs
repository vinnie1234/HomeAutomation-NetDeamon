namespace Automation.apps.General;

//[NetDaemonApp(Id = nameof(TestApp))]
// ReSharper disable once UnusedType.Global
//[Focus]
public class TestApp : BaseApp
{
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public TestApp(IHaContext ha, ILogger<TestApp> logger, INotify notify, INetDaemonScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        Notify.NotifyHouse("test");
    }
}