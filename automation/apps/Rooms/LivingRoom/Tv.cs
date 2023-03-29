namespace Automation.apps.Rooms.LivingRoom;

[NetDaemonApp(Id = nameof(Tv))]
// ReSharper disable once UnusedType.Global
public class Tv : BaseApp
{
    private bool IsMorningTime => Entities.InputSelect.Housemodeselect.State == "Morning";
    private bool IsDayTime => Entities.InputSelect.Housemodeselect.State == "Day";
    private bool IsEveningTime => Entities.InputSelect.Housemodeselect.State == "Evening";
    private bool IsNightTime => Entities.InputSelect.Housemodeselect.State == "Night";
    private bool IsWorking => Entities.InputBoolean.Working.IsOn();

    private bool DisableLightAutomations => Entities.InputBoolean.Disablelightautomationlivingroom.IsOn();

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public Tv(IHaContext ha, ILogger<Tv> logger, INotify notify)
        : base(ha, logger, notify)
    {
        Entities.MediaPlayer.Tv.WhenTurnsOn(_ => MovieTime());
        Entities.MediaPlayer.Tv.WhenTurnsOff(_ => LetThereBeLight());
    }

    private void LetThereBeLight()
    {
        Logger.LogDebug("TV Turned off");
        if (!DisableLightAutomations)
        {
            if (IsMorningTime)
                Entities.Scene.WoonkamerMorning.TurnOn();
            if (IsDayTime)
                Entities.Scene.WoonkamerDay.TurnOn();
            if (IsEveningTime)
                Entities.Scene.WoonkamerEvening.TurnOn();
            if (IsNightTime)
                Entities.Scene.WoonkamerNight.TurnOn();

            Entities.MediaPlayer.AvSoundbar.TurnOff();

            if (IsWorking)
            {
                Entities.Light.Plafond.TurnOn();
            }
        }
    }

    private void MovieTime()
    {
        Logger.LogDebug("TV Turned on");
        if (!DisableLightAutomations)
        {
            Entities.Scene.WoonkamerMovie.TurnOn();
            Entities.MediaPlayer.AvSoundbar.TurnOn();
            Entities.Light.PlafondWoonkamer.TurnOff();
            Entities.Light.HueFilamentBulb1.TurnOff();
            Entities.Light.HueFilamentBulb2.TurnOff();
            Entities.Light.Plafond.TurnOff();
            Entities.Light.Nachtkastje.TurnOff();
            Entities.MediaPlayer.Tv.VolumeSet(0.14);
            Logger.LogDebug("Movie time started!");
        }
    }
}