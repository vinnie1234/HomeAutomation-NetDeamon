using System.Collections;
using Automation.Enum;
using Newtonsoft.Json;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(HouseStateManager))]
public class HouseStateManager : BaseApp
{
    private TimeSpan _nighttimeWeekdays;
    private TimeSpan _nighttimeWeekends;
    private TimeSpan _daytimeWeekend;
    private TimeSpan _daytimeHomeWork;
    private TimeSpan _daytimeOffice;
    private readonly TimeSpan _startWorking = TimeSpan.Parse("08:30:00");
    private readonly TimeSpan _endWorking = TimeSpan.Parse("17:00:00");

    public HouseStateManager(IHaContext ha, ILogger<HouseStateManager> logger,
        INotify notify, INetDaemonScheduler scheduler)
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
        SetHouseState(Entities.Sun.Sun.State == "below_horizon" ? HouseStateEnum.Evening : HouseStateEnum.Day);
    }

    private void SetTimes()
    {
        _nighttimeWeekdays = TimeSpan.Parse(Entities.InputDatetime.Nighttimeweekdays.State ?? "00:00:00");
        _nighttimeWeekends = TimeSpan.Parse(Entities.InputDatetime.Nighttimeweekends.State ?? "00:30:00");
        _daytimeWeekend = TimeSpan.Parse(Entities.InputDatetime.Daytimeweekend.State ?? "10:00:00");
        _daytimeHomeWork = TimeSpan.Parse(Entities.InputDatetime.Daytimehomework.State ?? "08:15:00");
        _daytimeOffice = TimeSpan.Parse(Entities.InputDatetime.Daytimeoffice.State ?? "07:15:00");
    }

    /// <summary>
    ///     Sets the house state on the corresponding scene
    /// </summary>
    private void InitHouseStateSceneManagement()
    {
        //todo is unknown
        Entities.Scene.Woonkamerday.WhenTurnsOn(_ => SetHouseState(HouseStateEnum.Day));
        Entities.Scene.Woonkamerevening.WhenTurnsOn(_ => SetHouseState(HouseStateEnum.Evening));
        Entities.Scene.Woonkamernight.WhenTurnsOn(_ => SetHouseState(HouseStateEnum.Night));
        Entities.Scene.Woonkamermorning.WhenTurnsOn(_ => SetHouseState(HouseStateEnum.Morning));
    }

    private void SetSleepingOffFromAlarm()
    {
        Scheduler.RunDaily(TimeSpan.Parse("00:00:00"), () =>
        {
            var alarmsHubJson = JsonConvert.SerializeObject(Entities.Sensor.HubVincentAlarms.Attributes?.Alarms);
            var alarmsHub = JsonConvert.DeserializeObject<List<AlarmStateModel>>(alarmsHubJson);
            var alarmToday =
                alarmsHub?.FirstOrDefault(x =>
                    !string.IsNullOrEmpty(x.LocalTime) &&
                    DateTimeOffset.Parse(x.LocalTime).Date == DateTimeOffset.Now.Date);

            if (alarmToday is { LocalTime: not null })
                Scheduler.RunOnce(DateTimeOffset.Parse(alarmToday.LocalTime), () =>
                {
                    Logger.LogDebug(@"Setting schedular for {Time}. Sleeping off from alarm",
                        alarmToday.LocalTime);
                    Entities.InputBoolean.Sleeping.TurnOff();
                });
        });
    }

    private void SetWorking()
    {
        Scheduler.RunDaily(_startWorking, () =>
        {
            if (((IList)Globals.OfficeDays).Contains(DateTime.Now.DayOfWeek) ||
                ((IList)Globals.HomeWorkDays).Contains(DateTime.Now.DayOfWeek) &&
                Entities.InputBoolean.Holliday.IsOff())
            {
                Entities.InputBoolean.Working.TurnOn();
            }
        });

        Scheduler.RunDaily(_endWorking, () =>
        {
            if (((IList)Globals.OfficeDays).Contains(DateTime.Now.DayOfWeek) ||
                ((IList)Globals.HomeWorkDays).Contains(DateTime.Now.DayOfWeek) &&
                Entities.InputBoolean.Holliday.IsOff())
            {
                Entities.InputBoolean.Working.TurnOff();
            }
        });
    }

    private void SetDayTime()
    {
        Scheduler.RunDaily(_daytimeOffice, () =>
        {
            if (((IList)Globals.OfficeDays).Contains(DateTime.Now.DayOfWeek) && Entities.InputBoolean.Holliday.IsOff())
                SetHouseState(HouseStateEnum.Day);
        });
        Scheduler.RunDaily(_daytimeHomeWork, () =>
        {
            if (((IList)Globals.HomeWorkDays).Contains(DateTime.Now.DayOfWeek) &&
                Entities.InputBoolean.Holliday.IsOff())
                SetHouseState(HouseStateEnum.Day);
        });

        Scheduler.RunDaily(_daytimeWeekend, () =>
        {
            if (((IList)Globals.WeekendDays).Contains(DateTime.Now.DayOfWeek) || Entities.InputBoolean.Holliday.IsOn())
                SetHouseState(HouseStateEnum.Day);
        });
    }

    /// <summary>
    ///     Set night time schedule on different time different weekdays
    /// </summary>
    private void SetNightTime()
    {
        Entities.InputBoolean.Sleeping.WhenTurnsOn(_ => SetHouseState(HouseStateEnum.Night));
        Scheduler.RunDaily(_nighttimeWeekdays, () =>
        {
            if (((IList)Globals.WeekdayNightDays).Contains(DateTime.Now.DayOfWeek))
                SetHouseState(HouseStateEnum.Night);
        });

        Scheduler.RunDaily(_nighttimeWeekends, () =>
        {
            if (((IList)Globals.WeekendNightDays).Contains(DateTime.Now.DayOfWeek))
                SetHouseState(HouseStateEnum.Night);
        });
    }

    /// <summary>
    ///     Set to evening when the sun is down
    /// </summary>
    private void SetEveningWhenSunIsDown()
    {
        Entities.Sun.Sun.StateChanges().Where(x => x.Entity.State == "below_horizon").Subscribe(_ =>
        {
            Logger.LogDebug(@"Setting current house state to {State}", HouseStateEnum.Evening);
            SetHouseState(HouseStateEnum.Evening);
        });
    }

    /// <summary>
    ///     When sun is up considered morning time
    /// </summary>
    private void SetMorningWhenSunIsUp()
    {
        Entities.Sun.Sun.StateChanges().Where(x => x.Entity.State == "above_horizon").Subscribe(_ =>
        {
            Logger.LogDebug(@"Setting current house state to {State}", HouseStateEnum.Morning);
            SetHouseState(HouseStateEnum.Morning);
        });
    }

    /// <summary>
    ///     Sets the house state to specified state and updates Home Assistant InputSelect
    /// </summary>
    /// <param name="stateEnum">State to set</param>
    private void SetHouseState(HouseStateEnum stateEnum)
    {
        Logger.LogDebug(@"Setting current house state to {State}", stateEnum);
        var selectState = stateEnum switch
        {
            HouseStateEnum.Morning => "Morning",
            HouseStateEnum.Day     => "Day",
            HouseStateEnum.Evening => "Evening",
            HouseStateEnum.Night   => "Night",
            _                      => throw new ArgumentException("Not supported", nameof(stateEnum))
        };
        Entities.InputSelect.Housemodeselect.SelectOption(option: selectState);
    }
}