using System.Reactive.Concurrency;
using Automation.Enum;
using static Automation.Globals;

namespace Automation.apps.Rooms.LivingRoom;

[NetDaemonApp(Id = nameof(Tv))]
[Focus]
public class Tv : BaseApp
{
    private bool IsWorking => Entities.InputBoolean.Working.IsOn();

    private bool DisableLightAutomations => Entities.InputBoolean.Disablelightautomationlivingroom.IsOn();

    public Tv(
        IHaContext ha,
        ILogger<Tv> logger,
        INotify notify,
        IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        Entities.MediaPlayer.Tv.WhenTurnsOn(_ => MovieTime());
        Entities.MediaPlayer.Tv.WhenTurnsOff(_ => LetThereBeLight());
        
        //Green is broken, red is always good
        Entities.Light.Bank
            .StateAllChanges()
            .Where(x => x.New.IsOn())
            .Subscribe(x =>
            {
                x.Entity.TurnOn(rgbColor: new List<int> { 255, 0, 0 });
            });
    }

    private void LetThereBeLight()
    {
        Logger.LogDebug("TV Turned off");

        if (DisableLightAutomations) return;
        
        switch (GetHouseState(Entities))
        {
            case HouseState.Morning:
                Entities.Scene.Woonkamermorning.TurnOn();
                break;
            case HouseState.Day:
                Entities.Scene.Woonkamerday.TurnOn();
                break;
            case HouseState.Evening:
                Entities.Scene.Woonkamerevening.TurnOn();
                break;
            case HouseState.Night:
                Entities.Scene.Woonkamernight.TurnOn();
                break;
            default:
                Entities.Scene.Woonkamerday.TurnOn();
                break;
        }

        Entities.MediaPlayer.AvSoundbar.TurnOff();
        Entities.Switch.Ps5VincentPower.TurnOff();

        if (IsWorking) Entities.Light.Plafond.TurnOn();
    }

    private void MovieTime()
    {
        Logger.LogDebug("TV Turned on");

        if (!DisableLightAutomations)
        {
            Entities.Scene.WoonkamerMovie2.TurnOn();
            Entities.MediaPlayer.AvSoundbar.TurnOn();
            Entities.Light.PlafondWoonkamer.TurnOff();
            Entities.Light.HueFilamentBulb1.TurnOff();
            Entities.Light.HueFilamentBulb2.TurnOff();
            Entities.Light.LampenKeuken.TurnOff();
            Entities.Light.Plafond.TurnOff();
            Entities.Light.Nachtkastje.TurnOff();
            Entities.MediaPlayer.Tv.VolumeSet(0.14);
            Logger.LogDebug("Movie time started!");
        }
    }
}