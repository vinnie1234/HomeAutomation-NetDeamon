using NetDaemon.Extensions.Scheduler;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(HolidayManager))]
// ReSharper disable once UnusedType.Global
public class HolidayManager : BaseApp
{
    private readonly INetDaemonScheduler _scheduler;

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public HolidayManager(IHaContext ha, ILogger<HolidayManager> logger, INetDaemonScheduler scheduler, INotify notify)
        : base(ha, logger, notify)
    {
        _scheduler = scheduler;

        Entities.InputBoolean.Holliday.StateChanges().Where(x => x.Entity.IsOn()).Subscribe(_ => SetHoliday());
        Entities.InputBoolean.Holliday.StateChanges().Where(x => x.Entity.IsOff()).Subscribe(_ => SetEndHoliday());

        CheckCalenderForHoliday();
    }

    private void SetHoliday()
    {
        if (Entities.Sensor.HubVincentAlarms.Attributes is { NextAlarmStatus: "set", Alarms: { } })
        {
            var alarmList = new List<AlarmStateModel>();
            var jsonList = Entities.Sensor.HubVincentAlarms.Attributes.Alarms;

            if (jsonList != null)
                alarmList.AddRange(from JsonElement o in jsonList select o.Deserialize<AlarmStateModel>());

            var firstAlarm = alarmList.OrderBy(x => x.LocalTime).FirstOrDefault(x => x.Status == "set");
            Notify.NotifyGsmVincent(@"WEKKER UITZETTEN",
                @$"Je moet je wekker nog uit zetten voor {DateTime.Parse(firstAlarm?.LocalTime ?? string.Empty):dd-MM-yyyy hh:mm}");

            Logger.LogDebug("Send reminder for disable alarm");
        }
    }

    private void SetEndHoliday()
    {
        if (Entities.Sensor.HubVincentAlarms.Attributes?.NextAlarmStatus == "inactive")
        {
            Notify.NotifyGsmVincent(@"WEKKER AANZETTEN", @"Helaas moet je je wekker nog aanzetten :(");
            Logger.LogDebug("Send reminder for enable alarm");
        }
    }

    private void CheckCalenderForHoliday()
    {
        _scheduler.RunDaily(TimeSpan.Parse("00:00:00"), () =>
        {
            Logger.LogDebug(@"Check calender for the word 'vrij'");
            if (Entities.Calendar.VincentmaarschalkerweerdGmailCom.Attributes?.Description?.ToLower()
                    .Contains(@"vrij") ?? false)
            {
                Logger.LogDebug(@"Find the word 'vrij' and changed holiday sate");
                Entities.InputBoolean.Holliday.TurnOn();
            }
        });
    }
}