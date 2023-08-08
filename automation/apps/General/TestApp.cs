using System.Reactive.Concurrency;
using NetDaemon.Client;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(TestApp))]
//ReSharper disable once UnusedType.Global
//[Focus]
public class TestApp : BaseApp
{
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public TestApp(IHaContext ha, IHomeAssistantConnection homeAssistantConnection, ILogger<TestApp> logger, INotify notify, IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        Notify.NotifyHouse("welcomeHome", @"Doet dit het?", true);
    }
}