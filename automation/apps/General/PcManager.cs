using System.Reactive.Concurrency;
using System.Threading;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(PcManager))]
public class PcManager : BaseApp
{
    public PcManager(
        IHaContext ha, 
        ILogger<PcManager> logger, 
        INotify notify,
        IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        Entities.InputButton.StartPc
            .StateChanges()
            .Subscribe(_ =>
            {
                Entities.Light.Bureau.TurnOn();
                Entities.Light.Nachtkastje.TurnOff();
                Entities.Light.Plafond.TurnOn();
                Entities.MediaPlayer.Tv.TurnOff();
            });
        
        Entities.Sensor.VincentPcLaatstopgestart
            .StateChanges()
            .Subscribe(_ =>
            {
                Entities.Light.Bureau.TurnOn();
                Entities.Light.Nachtkastje.TurnOff();
                Entities.Light.Plafond.TurnOn();
                Entities.MediaPlayer.Tv.TurnOff();
            });

        Entities.Button.VincentPcAfsluiten
            .StateChanges()
            .Subscribe(_ =>
            {
                LightExtension.TurnOnLightsWoonkamer(Entities, scheduler);
                
                Thread.Sleep(TimeSpan.FromMinutes(1));
                Entities.Light.Bureau.TurnOff();
            });
    }
}