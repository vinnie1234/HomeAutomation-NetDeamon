using System.Reactive.Concurrency;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(HolidayManager))]
public class HolidayManager : BaseApp
{
    public HolidayManager(
        IHaContext ha, 
        ILogger<HolidayManager> logger, 
        INotify notify, 
        IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        HolidayChangeStateHandler();

        CheckCalenderForHoliday();
    }

    private void HolidayChangeStateHandler()
    {
        Entities
            .InputBoolean
            .Holliday
            .StateChanges()
            .Where(x =>
                x.Entity.IsOn())
            .Subscribe(_ => SetHoliday());

        Entities
            .InputBoolean
            .Holliday
            .StateChanges()
            .Where(x =>
                x.Entity.IsOff())
            .Subscribe(_ => SetEndHoliday());
    }

    private void SetHoliday()
    {
        if (Entities.Sensor.HubVincentAlarms.Attributes is { NextAlarmStatus: "set", Alarms: not null })
        {
            var alarmList = new List<AlarmStateModel?>();
            var jsonList = Entities.Sensor.HubVincentAlarms.Attributes.Alarms;

            if (jsonList != null)
                alarmList.AddRange(
                    jsonList.Cast<JsonElement>()
                        .Select(o => o.Deserialize<AlarmStateModel>()));

            var firstAlarm = alarmList.Where(x => x?.Status == "set").MinBy(x => x?.LocalTime);
            Notify.NotifyPhoneVincent(@"WEKKER UITZETTEN",
                @$"Je moet je wekker nog uit zetten voor {firstAlarm?.LocalTime ?? ""}", true);

            Logger.LogDebug("Send reminder for disable alarm");
        }
    }

    private void SetEndHoliday()
    {
        if (Entities.Sensor.HubVincentAlarms.Attributes?.NextAlarmStatus == "inactive")
        {
            Notify.NotifyPhoneVincent(@"WEKKER AANZETTEN", @"Helaas moet je je wekker nog aanzetten :(", true);
            Logger.LogDebug("Send reminder for enable alarm");
        }
    }

    private void CheckCalenderForHoliday()
    {
        Scheduler.ScheduleCron("00 00 * * *", () =>
        {
            var description = Entities.Calendar.VincentmaarschalkerweerdGmailCom.Attributes?.Description?.ToLower();
            if (description?.Contains(@"vrij") == true || description?.Contains(@"vakantie") == true) 
                Entities.InputBoolean.Holliday.TurnOn();
        });
    }
}