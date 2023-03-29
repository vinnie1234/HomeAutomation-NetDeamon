namespace Automation.apps.Rooms.BathRoom;

[NetDaemonApp(Id = nameof(BathRoomLightOnMovement))]
// ReSharper disable once UnusedType.Global
public class BathRoomLightOnMovement : BaseApp
{
    private bool IsNighttime => Entities.InputSelect.Housemodeselect.State == "Night";
    private bool IsSleeping => Entities.InputBoolean.Sleeping.IsOn();
    private bool DisableLightAutomations => Entities.InputBoolean.Disablelightautomationbathroom.IsOn();
    private bool IsDouching => Entities.InputBoolean.Douchen.IsOn();

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public BathRoomLightOnMovement(IHaContext ha, ILogger<BathRoomLightOnMovement> logger, INotify notify)
        : base(ha, logger, notify)
    {
        HaContext.Events.Where(x => x.EventType == "hue_event").Subscribe(x =>
        {
            var eventModel = x.DataElement?.ToObject<EventModel>();
            if (eventModel != null) OverwriteSwitch(eventModel);
        });

        InitializeLights();
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
                TimeSpan.FromMinutes((int)Entities.InputNumber.Bathroomlightnighttime.State!))
            .Where(x => x.Old.IsOn() && !DisableLightAutomations && !IsDouching && IsNighttime && IsSleeping)
            .Subscribe(_ => ChangeLight(false));

        Entities.BinarySensor.BadkamerMotion
            .StateChanges()
            .WhenStateIsFor(x => x.IsOff(), TimeSpan.FromMinutes((int)Entities.InputNumber.Bathroomlightdaytime.State!))
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
            Entities.Light.BadkamerSpiegel.TurnOn(brightnessPct: 100);
            Entities.Light.PlafondBadkamer.TurnOn(brightnessPct: 100);
            Entities.Cover.Rollerblind0001.CloseCover();
            Notify.NotifyHouse(@"Tijd om te douchen");
        }
        else
        {
            Entities.Light.BadkamerSpiegel.TurnOff();
            Entities.Light.PlafondBadkamer.TurnOff();
            Entities.Cover.Rollerblind0001.OpenCover();
            Notify.NotifyHouse(@"Klaar met douchen");
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
        Logger.LogDebug("Turn bathroom light {On} with brightnessPct {BrightnessPct}", on, brightnessPct);

        switch (on)
        {
            case true:
                Entities.Light.PlafondBadkamer.TurnOn(brightnessPct: brightnessPct, transition: 2);
                Entities.Light.BadkamerSpiegel.TurnOn(brightnessPct: brightnessPct, transition: 2);
                break;
            case false:
                Entities.Light.Badkamer.TurnOff();
                break;
        }
    }

    private void OverwriteSwitch(EventModel eventModel)
    {
        if (eventModel is { DeviceId: "3dcab87acc97379282b359fdf3557a52", Type: "initial_press" })
        {
            switch (eventModel.Subtype)
            {
                case 1:
                    Entities.Light.BadkamerSpiegel.TurnOff();
                    Entities.Light.PlafondBadkamer.TurnOff();
                    break;
                case 2:
                    Entities.Light.BadkamerSpiegel.TurnOn(brightnessStepPct: 10);
                    Entities.Light.PlafondBadkamer.TurnOn(brightnessStepPct: 10);
                    break;
                case 3:
                    Entities.Light.BadkamerSpiegel.TurnOn(brightnessStepPct: -10);
                    Entities.Light.PlafondBadkamer.TurnOn(brightnessStepPct: -10);
                    break;
                case 4:
                    if (Entities.InputBoolean.Douchen.IsOn()) Entities.InputBoolean.Douchen.TurnOff();
                    if (Entities.InputBoolean.Douchen.IsOff()) Entities.InputBoolean.Douchen.TurnOn();
                    break;
            }
        }
    }
}