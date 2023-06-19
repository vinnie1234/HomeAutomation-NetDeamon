using System.Reactive.Concurrency;

namespace Automation.Extensions;

public static class SchedulerExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IDisposable RunDaily(this IScheduler scheduler, TimeSpan timeOfDay, Action action)
    {
        var startTime = scheduler.Now.Date.Add(timeOfDay);
        if (scheduler.Now > startTime)
        {
            startTime = startTime.AddDays(1);
        }

        return scheduler.RunEvery(TimeSpan.FromDays(1), startTime, action);
    }
}