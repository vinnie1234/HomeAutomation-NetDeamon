using System.Reactive.Concurrency;
using System.Text.RegularExpressions;
using Automation.Models.Yts;
using NetDaemon.Client;
using Newtonsoft.Json;

// ReSharper disable UnusedParameter.Local

namespace Automation.apps.General;

//[NetDaemonApp(Id = nameof(TestApp))]
//[Focus]
//ReSharper disable once UnusedType.Global
// ReSharper disable once ClassNeverInstantiated.Global
public class TestApp : BaseApp
{
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public TestApp(IHaContext ha, IHomeAssistantConnection homeAssistantConnection, ILogger<TestApp> logger,
        INotify notify, IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
    }


}