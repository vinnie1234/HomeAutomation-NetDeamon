using System.Reactive.Concurrency;

namespace Automation.apps.Rooms.BathRoom;

[NetDaemonApp(Id = nameof(BathRoomLights))]
public class BathRoomLights : BaseApp
{
    private bool IsNighttime => Entities.InputSelect.Housemodeselect.State == "Night";
    private bool IsSleeping => Entities.InputBoolean.Sleeping.IsOn();
    private bool DisableLightAutomations => Entities.InputBoolean.Disablelightautomationbathroom.IsOn();
    private bool IsDouching => Entities.InputBoolean.Douchen.IsOn();

    public BathRoomLights(
        IHaContext ha,
        ILogger<BathRoomLights> logger,
        INotify notify,
        IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        HaContext.Events.Where(x => x.EventType == "hue_event").Subscribe(x =>
        {
            var eventModel = x.DataElement?.ToObject<EventModel>();
            if (eventModel != null) OverwriteSwitch(eventModel);
        });

        InitializeLights();
        ToothbrushHandler();
    }

    private void InitializeLights()
    {
        Entities.BinarySensor.BadkamerMotion
            .StateChanges()
            .Where(x => x.Old.IsOff() && x.New.IsOn() && !DisableLightAutomations)
            .Subscribe(_ => ChangeLight(true, GetBrightness()));

        Entities.BinarySensor.BadkamerMotion
            .StateChanges()
            .WhenStateIsFor(x => x.IsOff(),
                TimeSpan.FromMinutes((int)Entities.InputNumber.Bathroomlightnighttime.State!), Scheduler)
            .Where(x => x.Old.IsOn() && !DisableLightAutomations && !IsDouching && IsNighttime && IsSleeping)
            .Subscribe(_ => ChangeLight(false));

        Entities.BinarySensor.BadkamerMotion
            .StateChanges()
            .WhenStateIsFor(x => x.IsOff(), TimeSpan.FromMinutes((int)Entities.InputNumber.Bathroomlightdaytime.State!),
                Scheduler)
            .Where(x => x.Old.IsOn() && !DisableLightAutomations && !IsDouching && !IsSleeping)
            .Subscribe(_ => ChangeLight(false));

        Entities.InputBoolean.Douchen
            .StateChanges()
            .Subscribe(x => DouchingAutomation(x.New.IsOn()));
    }

    private void DouchingAutomation(bool isOn)
    {
        if (isOn)
        {
            Entities.MediaPlayer.Googlehome0351.VolumeSet(0.40);
            Services.Spotcast.Start(entityId: Entities.MediaPlayer.Googlehome0351.EntityId);
            Entities.Light.BadkamerSpiegel.TurnOn(brightnessPct: 100);
            Entities.Light.PlafondBadkamer.TurnOn(brightnessPct: 100);
            Entities.Cover.Rollerblind0001.CloseCover();
            Notify.NotifyHouse("timeToDouche", "Tijd om te douchen", true);
            Scheduler.Schedule(TimeSpan.FromHours(1), () =>
            {
                if (IsDouching)
                {
                    Entities.MediaPlayer.Googlehome0351.MediaStop();
                    Entities.InputBoolean.Douchen.TurnOff();
                    Notify.NotifyHouse("toLongDouchen",
                        "Je bent of een uur aan het douchen, of weer is vergeten alles uit te zetten!", false, 60);
                }
            });
        }
        else
        {
            Entities.Light.BadkamerSpiegel.TurnOff();
            Entities.Light.PlafondBadkamer.TurnOff();
            Entities.Cover.Rollerblind0001.OpenCover();
            Entities.MediaPlayer.Googlehome0351.MediaPause();
            Notify.NotifyHouse("readyDouche", "Klaar met douchen", true);
        }
    }

    private int GetBrightness()
    {
        return IsSleeping switch
        {
            true  => 5,
            false => 100
        };
    }

    private void ChangeLight(bool on, int brightnessPct = 0)
    {
        switch (on)
        {
            case true:
                Entities.Light.PlafondBadkamer.TurnOn(brightnessPct: brightnessPct, transition: 2);
                Entities.Light.BadkamerSpiegel.TurnOn(brightnessPct: brightnessPct, transition: 2);
                break;
            case false:
                Entities.Light.PlafondBadkamer.TurnOff();
                Entities.Light.BadkamerSpiegel.TurnOff();
                break;
        }
    }

    private void OverwriteSwitch(EventModel eventModel)
    {
        const string hueSwitchBathroomId = "3dcab87acc97379282b359fdf3557a52";

        if (eventModel is { DeviceId: hueSwitchBathroomId, Type: "initial_press" })
            switch (eventModel.Subtype)
            {
                //button one
                case 1:
                    Entities.Light.BadkamerSpiegel.TurnOff();
                    Entities.Light.PlafondBadkamer.TurnOff();
                    break;
                //button two
                case 2:
                    Entities.Light.BadkamerSpiegel.TurnOn(brightnessStepPct: 10);
                    Entities.Light.PlafondBadkamer.TurnOn(brightnessStepPct: 10);
                    break;
                //button three
                case 3:
                    Entities.Light.BadkamerSpiegel.TurnOn(brightnessStepPct: -10);
                    Entities.Light.PlafondBadkamer.TurnOn(brightnessStepPct: -10);
                    break;
                //button four
                case 4:
                    if (Entities.InputBoolean.Douchen.IsOn()) Entities.InputBoolean.Douchen.TurnOff();
                    if (Entities.InputBoolean.Douchen.IsOff()) Entities.InputBoolean.Douchen.TurnOn();
                    break;
            }
    }

    private void ToothbrushHandler()
    {
        Entities.Sensor.SmartSeries400097aeToothbrushState
            .StateChanges()
            .Where(x => x.New?.State == "runnig")
            .Subscribe(_ =>
            {
                if (!IsDouching)
                {
                    Entities.MediaPlayer.Googlehome0351.VolumeSet(0.15);
                    Services.Spotcast.Start(entityId: Entities.MediaPlayer.Googlehome0351.EntityId);
                    Entities.MediaPlayer.Googlehome0351.MediaPlay();
                }
            });

        Entities.Sensor.SmartSeries400097aeToothbrushState
            .StateChanges()
            .WhenStateIsFor(x => x?.State == "idle" && Entities.InputBoolean.Away.IsOff(),
                TimeSpan.FromSeconds(30), Scheduler)
            .Subscribe(_ =>
            {
                if (!IsDouching)
                    Entities.MediaPlayer.Googlehome0351.MediaStop();
            });
    }
}