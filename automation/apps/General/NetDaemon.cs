using System.Reactive.Concurrency;
using System.Threading;
using Automation.Enum;
using Automation.Helpers;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(TestApp))]
//[Focus]
//ReSharper disable once UnusedType.Global
public class NetDaemon : BaseApp
{
    private readonly string _discordLogChannel = ConfigManager.GetValueFromConfigNested("Discord", "Logs") ?? "";
    
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public NetDaemon(IHaContext ha, ILogger<NetDaemon> logger,
        INotify notify, IScheduler scheduler, IDataRepository storage)
        : base(ha, logger, notify, scheduler)
    {
        var lightColor = storage.Get<object>("NetDaemonRestart");
        
        if (lightColor != null && lightColor.ToString() != "")
        {
            Entities.Light.Koelkast.TurnOn(rgbColor: lightColor);
            storage.Save("NetDaemonRestart", "");
        }
        
        
        if(!Entities.InputBoolean.Sleeping.IsOn())
            notify.NotifyHouse("Het huis is opnieuw opgestart", "Het huis is opnieuw opgestart", true);
        notify.NotifyDiscord("Het huis is opnieuw opgestart", new[] { _discordLogChannel });

        Entities.InputButton.Restartnetdaemon.StateChanges().Subscribe(_ =>
        {
            storage.Save("NetDaemonRestart", Entities.Light.Koelkast.Attributes?.RgbColor);
            Entities.Light.Koelkast.TurnOn(colorName: "red");
            notify.NotifyHouse("Het huis wordt opnieuw opgestart", "Het huis wordt opnieuw opgestart", true);
            
            Thread.Sleep(TimeSpan.FromSeconds(5));
            Services.Hassio.AddonRestart("c6a2317c_netdaemon4");
        });
    }
}