using System.Reactive.Concurrency;
using System.Threading;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(TestApp))]
//[Focus]
//ReSharper disable once UnusedType.Global
public class NetDaemon : BaseApp
{
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
        
        notify.NotifyHouse(@"Het huis is opnieuw opgestart", @"Het huis is opnieuw opgestart", true);

        Entities.InputButton.Restartnetdaemon.StateChanges().Subscribe(_ =>
        {
            storage.Save("NetDaemonRestart", Entities.Light.Koelkast.Attributes?.RgbColor);
            Entities.Light.Koelkast.TurnOn(colorName: "red");
            notify.NotifyHouse(@"Het huis wordt opnieuw opgestart", @"Het huis wordt opnieuw opgestart", true);
            
            Thread.Sleep(TimeSpan.FromSeconds(5));
            Services.Hassio.AddonRestart(@"c6a2317c_netdaemon4");
        });
    }
}