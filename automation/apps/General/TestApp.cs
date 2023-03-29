namespace Automation.apps.General;

//[NetDaemonApp(Id = nameof(TestApp))]
// ReSharper disable once UnusedType.Global
public class TestApp : BaseApp
{
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public TestApp(IHaContext ha, ILogger<TestApp> logger, INotify notify)
        : base(ha, logger, notify)
    {
        notify.NotifyGsmVincent("test", "test");
    }
}