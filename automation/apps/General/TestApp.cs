using System.Reactive.Concurrency;
using System.Threading;
using Automation.Helpers;
using NetDaemon.Client;
using NetDaemon.Client.HomeAssistant.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(TestApp))]
//ReSharper disable once UnusedType.Global
[Focus]
public class TestApp : BaseApp
{
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public TestApp(IHaContext ha, IHomeAssistantConnection homeAssistantConnection, ILogger<TestApp> logger, INotify notify, IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        Notify.NotifyHouse("welcomeHome", @"Doet dit het?", true);
    }
}