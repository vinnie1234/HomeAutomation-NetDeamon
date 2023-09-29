using System.Reactive.Concurrency;
using Automation.Models.DiscordNotificationModels;
using NetDaemon.Client;

namespace Automation.apps.General;

//[NetDaemonApp(Id = nameof(TestApp))]
//[Focus]
//ReSharper disable once UnusedType.Global
public class TestApp : BaseApp
{
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public TestApp(IHaContext ha, IHomeAssistantConnection homeAssistantConnection, ILogger<TestApp> logger,
        INotify notify, IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
    }
}