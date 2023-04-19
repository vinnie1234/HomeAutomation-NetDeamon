namespace Automation.Extensions;

public static class SchedulerExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IDisposable RunDaily(this INetDaemonScheduler scheduler, TimeSpan timeOfDay, Action action)
    {
        var startTime = scheduler.Now.Date.Add(timeOfDay);
        if (scheduler.Now > startTime)
        {
            startTime = startTime.AddDays(1);
        }

        return scheduler.RunEvery(TimeSpan.FromDays(1), startTime, action);
    }

    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IDisposable RunOnce(this INetDaemonScheduler scheduler, DateTimeOffset dateTime, Action action)
    {
        return scheduler.RunAt(dateTime, action);
    }
}