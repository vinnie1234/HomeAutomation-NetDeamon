using System.Reactive.Concurrency;

namespace Automation.apps.Rooms.Hall;

[NetDaemonApp(Id = nameof(HallLightOnMovement))]
public class HallLightOnMovement : BaseApp
{
    private bool IsNighttime => Entities.InputSelect.Housemodeselect.State == "Night";
    private bool IsSleeping => Entities.InputBoolean.Sleeping.IsOn();
    private bool DisableLightAutomations => Entities.InputBoolean.Disablelightautomationhall.IsOn();
    
    public HallLightOnMovement(IHaContext ha, ILogger<HallLightOnMovement> logger, INotify notify, IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        InitializeDayLights();
    }

    private void InitializeDayLights()
    {
        Entities.BinarySensor.GangMotion
            .StateChanges()
            .Where(x => x.New.IsOn() && !DisableLightAutomations)
            .Subscribe(_ => ChangeLight(true, GetBrightness()));

        Entities.BinarySensor.GangMotion
            .StateChanges()
            .WhenStateIsFor(x => x.IsOff(), TimeSpan.FromMinutes(GetStateTime()), Scheduler)
            .Where(_ => !DisableLightAutomations)
            .Subscribe(_ => ChangeLight(false));
    }

    private int GetBrightness()
    {
        return IsSleeping switch
        {
            true  => 5,
            false => 100
        };
    }

    private int GetStateTime()
    {
        if (IsNighttime && IsSleeping) return (int)Entities.InputNumber.Halllightnighttime.State!;
        if (!IsNighttime && !IsSleeping) return (int)Entities.InputNumber.Halllightdaytime.State!;

        return 1;
    }

    private void ChangeLight(bool on, int brightnessPct = 0)
    {
        switch (on)
        {
            case true:
                Entities.Light.Hal2.TurnOn(brightnessPct: brightnessPct, transition: 15);
                if (!IsNighttime && !IsSleeping) Entities.Light.Hal.TurnOn();
                break;
            case false:
                Entities.Light.Hal.TurnOff();
                Entities.Light.Hal2.TurnOff();
                break;
        }
    }
}