using System.Reactive.Concurrency;

namespace Automation.apps.Rooms.LivingRoom;

[NetDaemonApp(Id = nameof(Gaming))]
public class Gaming : BaseApp
{
    private bool DisableLightAutomations => Entities.InputBoolean.Disablelightautomationlivingroom.IsOn();
    
    public Gaming(
        IHaContext ha, 
        ILogger<Gaming> logger, 
        INotify notify, 
        IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        Entities.DeviceTracker.Sony.WhenTurnsOn(_ => GameSetUp());
    }

    private void GameSetUp()
    {
        Logger.LogDebug("Gaming lights");

        if (!DisableLightAutomations)
        {
            Entities.MediaPlayer.Tv.TurnOn();
            Entities.MediaPlayer.Tv.SelectSource("HDMI2");
            Entities.MediaPlayer.AvSoundbar.TurnOn();
            Entities.Light.PlafondWoonkamer.TurnOff();
            Entities.Light.Plafond.TurnOff();
            Entities.Light.Nachtkastje.TurnOff();
            Entities.Light.HueFilamentBulb1.TurnOff();
            Entities.Light.HueFilamentBulb2.TurnOff();
            Entities.MediaPlayer.Tv.VolumeSet(0.14);
        }
    }
}