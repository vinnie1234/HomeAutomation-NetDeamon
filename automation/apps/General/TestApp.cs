using System.Reactive.Concurrency;
using Automation.Helpers;

namespace Automation.apps.General;

//[NetDaemonApp(Id = nameof(TestApp))]
// ReSharper disable once UnusedType.Global
//[Focus]
public class TestApp : BaseApp
{
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public TestApp(IHaContext ha, ILogger<TestApp> logger, INotify notify, IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        var uri = ConfigManager.GetValueFromConfigNested("Discord", "Pixel");
        if (uri != null) Helpers.Discord.SendMessage(uri, @"Pixellll");
    }
}