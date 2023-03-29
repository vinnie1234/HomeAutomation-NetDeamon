namespace Automation.apps.Rooms.LivingRoom;

[NetDaemonApp(Id = nameof(Gaming))]
// ReSharper disable once UnusedType.Global
// ReSharper disable once ClassNeverInstantiated.Global
public class Gaming : BaseApp
{
    private bool DisableLightAutomations => Entities.InputBoolean.Disablelightautomationlivingroom.IsOn();

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public Gaming(IHaContext ha, ILogger<Gaming> logger, INotify notify)
        : base(ha, logger, notify)
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
            Entities.Light.Slaapkamer.TurnOff();
            Entities.Light.HueFilamentBulb1.TurnOff();
            Entities.Light.HueFilamentBulb2.TurnOff();
            Entities.MediaPlayer.Tv.VolumeSet(0.14);
        }
    }
}