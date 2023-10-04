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

        Entities.InputButton.Restartnetdaemon.StateChanges().Subscribe(_ =>
        {
            storage.Save("NetDaemonRestart", Entities.Light.Koelkast.Attributes?.RgbColor);
            Entities.Light.Koelkast.TurnOn(colorName: "red");
            
            Thread.Sleep(TimeSpan.FromSeconds(2));
            Services.Hassio.AddonRestart(@"c6a2317c_netdaemon3_1");
        });
    }
}