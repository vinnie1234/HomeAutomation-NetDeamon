using System.Globalization;
using System.Reactive.Concurrency;
using Automation.Enum;
using Newtonsoft.Json;
using static Automation.Globals;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(HouseStateManager))]
public class HouseStateManager : BaseApp
{
    private TimeSpan _nighttimeWeekdays;
    private TimeSpan _nighttimeWeekends;
    private TimeSpan _daytimeWeekend;
    private TimeSpan _daytimeHomeWork;
    private TimeSpan _daytimeOffice;
    private readonly TimeSpan _startWorking = TimeSpan.Parse("08:30:00", new CultureInfo("nl-Nl"));
    private readonly TimeSpan _endWorking = TimeSpan.Parse("17:00:00", new CultureInfo("nl-Nl"));

    public HouseStateManager(
        IHaContext ha,
        ILogger<HouseStateManager> logger,
        INotify notify,
        IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        SetSleepingOffFromAlarm();
        SetTimes();
        SetDayTime();
        SetEveningWhenSunIsDown();
        SetNightTime();
        SetMorningWhenSunIsUp();
        InitHouseStateSceneManagement();
        SetCurrentStates();
        SetWorking();
    }


    private void SetCurrentStates()
    {
        SetHouseState(Entities.Sun.Sun.State == "below_horizon" ? HouseState.Evening : HouseState.Day);
    }

    private void SetTimes()
    {
        _nighttimeWeekdays = TimeSpan.Parse(Entities.InputDatetime.Nighttimeweekdays.State ?? "00:00:00", new CultureInfo("nl-Nl"));
        _nighttimeWeekends = TimeSpan.Parse(Entities.InputDatetime.Nighttimeweekends.State ?? "00:30:00", new CultureInfo("nl-Nl"));
        _daytimeWeekend = TimeSpan.Parse(Entities.InputDatetime.Daytimeweekend.State ?? "10:00:00", new CultureInfo("nl-Nl"));
        _daytimeHomeWork = TimeSpan.Parse(Entities.InputDatetime.Daytimehomework.State ?? "08:15:00", new CultureInfo("nl-Nl"));
        _daytimeOffice = TimeSpan.Parse(Entities.InputDatetime.Daytimeoffice.State ?? "07:15:00", new CultureInfo("nl-Nl"));
    }

    /// <summary>
    ///     Sets the house state on the corresponding scene
    /// </summary>
    private void InitHouseStateSceneManagement()
    {
        Entities.Scene.Woonkamerday.StateChanges().Subscribe(_ => SetHouseState(HouseState.Day));
        Entities.Scene.Woonkamerevening.StateChanges().Subscribe(_ => SetHouseState(HouseState.Evening));
        Entities.Scene.Woonkamernight.StateChanges().Subscribe(_ => SetHouseState(HouseState.Night));
        Entities.Scene.Woonkamermorning.StateChanges().Subscribe(_ => SetHouseState(HouseState.Morning));
    }

    private void SetSleepingOffFromAlarm()
    {
        Scheduler.ScheduleCron("00 00 * * *", () =>
        {
            var alarmsHubJson = JsonConvert.SerializeObject(Entities.Sensor.HubVincentAlarms.Attributes?.Alarms);
            var alarmsHub = JsonConvert.DeserializeObject<List<AlarmStateModel>>(alarmsHubJson);
            var alarmToday =
                alarmsHub?.Find(alarmStateModel =>
                    !string.IsNullOrEmpty(alarmStateModel.LocalTime) &&
                    DateTimeOffset.Parse(alarmStateModel.LocalTime, new CultureInfo("nl-Nl")).Date == DateTimeOffset.Now.Date);

            if (alarmToday is { LocalTime: not null })
                Scheduler.Schedule(DateTimeOffset.Parse(alarmToday.LocalTime, new CultureInfo("nl-Nl")), () =>
                {
                    Logger.LogDebug("Setting schedular for {Time}. Sleeping off from alarm",
                        alarmToday.LocalTime);
                    Entities.InputBoolean.Sleeping.TurnOff();
                });
        });
    }

    private void SetWorking()
    {
        Scheduler.RunDaily(_startWorking, () =>
        {
            if (OfficeDays.Contains(DateTimeOffset.Now.DayOfWeek) ||
                HomeWorkDays.Contains(DateTimeOffset.Now.DayOfWeek) &&
                Entities.InputBoolean.Holliday.IsOff())
                Entities.InputBoolean.Working.TurnOn();
        });

        Scheduler.RunDaily(_endWorking, () =>
        {
            if (OfficeDays.Contains(DateTimeOffset.Now.DayOfWeek) ||
                HomeWorkDays.Contains(DateTimeOffset.Now.DayOfWeek) &&
                Entities.InputBoolean.Holliday.IsOff())
                Entities.InputBoolean.Working.TurnOff();
        });
    }

    private void SetDayTime()
    {
        Scheduler.RunDaily(_daytimeOffice, () =>
        {
            if (OfficeDays.Contains(DateTimeOffset.Now.DayOfWeek) &&
                Entities.InputBoolean.Holliday.IsOff())
                SetHouseState(HouseState.Day);
        });
        Scheduler.RunDaily(_daytimeHomeWork, () =>
        {
            if (HomeWorkDays.Contains(DateTimeOffset.Now.DayOfWeek) &&
                Entities.InputBoolean.Holliday.IsOff())
                SetHouseState(HouseState.Day);
        });

        Scheduler.RunDaily(_daytimeWeekend, () =>
        {
            if (WeekendDays.Contains(DateTimeOffset.Now.DayOfWeek) ||
                Entities.InputBoolean.Holliday.IsOn())
                SetHouseState(HouseState.Day);
        });
    }

    /// <summary>
    ///     Set night time schedule on different time different weekdays
    /// </summary>
    private void SetNightTime()
    {
        Entities.InputBoolean.Sleeping.WhenTurnsOn(_ => SetHouseState(HouseState.Night));

        Scheduler.RunDaily(_nighttimeWeekdays, () =>
        {
            if (WeekdayNightDays.Contains(DateTimeOffset.Now.DayOfWeek))
                SetHouseState(HouseState.Night);
        });

        Scheduler.RunDaily(_nighttimeWeekends, () =>
        {
            if (WeekendNightDays.Contains(DateTimeOffset.Now.DayOfWeek))
                SetHouseState(HouseState.Night);
        });
    }

    /// <summary>
    ///     Set to evening when the sun is down
    /// </summary>
    private void SetEveningWhenSunIsDown()
    {
        Entities.Sun.Sun
            .StateChanges()
            .Where(change => change.Entity.State == "below_horizon")
            .Subscribe(_ =>
            {
                Logger.LogDebug("Setting current house state to {State}", HouseState.Evening);
                SetHouseState(HouseState.Evening);
            });
    }

    /// <summary>
    ///     When sun is up considered morning time
    /// </summary>
    private void SetMorningWhenSunIsUp()
    {
        Entities.Sun.Sun
            .StateChanges()
            .Where(change => change.Entity.State == "above_horizon")
            .Subscribe(_ =>
            {
                Logger.LogDebug("Setting current house state to {State}", HouseState.Morning);
                SetHouseState(HouseState.Morning);
            });
    }

    /// <summary>
    ///     Sets the house state to specified state and updates Home Assistant InputSelect
    /// </summary>
    /// <param name="state">State to set</param>
    private void SetHouseState(HouseState state)
    {
        Logger.LogDebug("Setting current house state to {State}", state);
        var selectState = state
            switch
            {
                HouseState.Morning => "Morning",
                HouseState.Day     => "Day",
                HouseState.Evening => "Evening",
                HouseState.Night   => "Night",
                _                      => throw new ArgumentException("Not supported", nameof(state))
            };
        Entities.InputSelect.Housemodeselect.SelectOption(option: selectState);
    }
}