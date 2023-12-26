using System.Reactive.Concurrency;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(AwayManager))]
public class PcManager : BaseApp
{
    public PcManager(
        IHaContext ha, 
        ILogger<PcManager> logger, 
        INotify notify,
        IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        Entities.Sensor.PcGeheugengebruik
            .StateChanges()
            .Where(x => x.New?.State > 0)
            .Subscribe(_ =>
            {
                Entities.Light.Nachtkastje.TurnOff();
                Entities.Light.Plafond.TurnOn();
                Entities.MediaPlayer.Tv.TurnOff();
            });
    }
}