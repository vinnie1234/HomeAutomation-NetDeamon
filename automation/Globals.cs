using Automation.Enum;

namespace Automation;

public static class Globals
{
    #region DayOfWeekConfig

    public static readonly DayOfWeek[] WeekdayNightDays =
    {
        DayOfWeek.Sunday,
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday,
        DayOfWeek.Thursday
    };

    public static readonly DayOfWeek[] WeekendNightDays =
    {
        DayOfWeek.Friday,
        DayOfWeek.Saturday
    };

    public static readonly DayOfWeek[] WeekendDays =
    {
        DayOfWeek.Saturday,
        DayOfWeek.Sunday
    };

    public static readonly DayOfWeek[] HomeWorkDays =
    {
        DayOfWeek.Monday,
        DayOfWeek.Tuesday,
        DayOfWeek.Wednesday
    };

    public static readonly DayOfWeek[] OfficeDays =
    {
        DayOfWeek.Thursday,
        DayOfWeek.Friday
    };

    #endregion

    public static HouseStateEnum GetHouseState(Entities entities)
    {
        return entities.InputSelect.Housemodeselect.State
            switch
            {
                "Morning" => HouseStateEnum.Morning,
                "Day"     => HouseStateEnum.Day,
                "Evening" => HouseStateEnum.Evening,
                "Night"   => HouseStateEnum.Night,
                _         => HouseStateEnum.Day
            };
    }

    public static bool AmIHomeCheck(Entities entities)
    {
        return entities.Person.VincentMaarschalkerweerd.State != "home";
    }
}