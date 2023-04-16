using System.Collections;
using NetDaemon.Extensions.Scheduler;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(SleepManager))]
// ReSharper disable once UnusedType.Global
public class SleepManager : BaseApp
{
    private bool DisableLightAutomations => Entities.InputBoolean.Disablelightautomationgeneral.IsOn();

    public SleepManager(IHaContext ha, ILogger<SleepManager> logger, INetDaemonScheduler scheduler, INotify notify)
        : base(ha, logger, notify)
    {
        Entities.InputBoolean.Sleeping.WhenTurnsOff(_ => WakeUp());
        Entities.InputBoolean.Sleeping.WhenTurnsOn(_ => Sleeping());

        scheduler.RunDaily(TimeSpan.Parse("10:00:00"), () =>
        {
            if (!((IList)Globals.WeekendDays).Contains(DateTime.Now.DayOfWeek))
            {
                if (Entities.InputBoolean.Sleeping.IsOn()) Entities.InputBoolean.Sleeping.TurnOff();
            }
        });
    }

    private void WakeUp()
    {
        Logger.LogDebug("Wake up Routine");
        if (((IList)Globals.WeekendDays).Contains(DateTime.Now.DayOfWeek))
        {
            Entities.Cover.Rollerblind0001.SetCoverPosition(100);
        }
        else if (Entities.Cover.Rollerblind0001.Attributes?.CurrentPosition < 100)
        {
            Entities.Cover.Rollerblind0001.SetCoverPosition(45);
        }

        SendBatteryWarning();
    }

    private void Sleeping()
    {
        Logger.LogDebug("Sleep Routine started");

        ChangeRelevantHouseState();
        TurnAllLightsOut();
        SendBatteryWarning();
        Entities.Cover.Rollerblind0001.SetCoverPosition(0);
        var checkDate = DateTime.Now;
        var message = Entities.Sensor.AfvalMorgen.State;
        if (checkDate.Hour is >= 00 and < 07)
        {
            message = Entities.Sensor.AfvalVandaag.State;
        }

        if (message != @"Geen")
        {
            Notify.NotifyGsmVincent(@"Vergeet het afval niet",
                @$"Vergeet je niet op {message} buiten te zetten?");
        }
    }

    private void ChangeRelevantHouseState()
    {
        Entities.InputBoolean.Away.TurnOff();
        Entities.InputBoolean.Douchen.TurnOff();
    }

    private void SendBatteryWarning()
    {
        if (Entities.Sensor.SmS908bBatteryLevel.State < 30)
        {
            if (Entities.BinarySensor.SmS908bIsCharging.IsOff())
                Notify.NotifyGsmVincent(@"Telefoon bijna leeg", @"Je moet je telefoon opladen");
        }

        if (Entities.Sensor.SmT860BatteryLevel.State < 30)
        {
            if (Entities.BinarySensor.SmT860IsCharging.IsOff())
                Notify.NotifyGsmVincent(@"Tabled bijna leeg", @"Je moet je tabled opladen");
        }
    }

    private void TurnAllLightsOut()
    {
        if (!DisableLightAutomations)
        {
            Entities.Light.TurnAllOff();
        }
    }
}