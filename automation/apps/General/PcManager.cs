using System.Reactive.Concurrency;

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
            .Throttle(TimeSpan.FromSeconds(90))
            .Subscribe(_ =>
            {
                Entities.Light.Bureau.TurnOff();
                LightExtension.TurnOnLightsWoonkamer(Entities, scheduler);
            });
    }
}