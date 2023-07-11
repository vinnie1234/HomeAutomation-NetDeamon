using System.Reactive.Concurrency;

namespace Automation.apps;

public class BaseApp
{
    internal readonly Entities Entities;
    internal readonly ILogger Logger;
    internal readonly INotify Notify;
    internal readonly IScheduler Scheduler;
    internal readonly IServices Services;
    internal readonly IHaContext HaContext;

    protected BaseApp(
        IHaContext haContext, 
        ILogger logger, 
        INotify notify, 
        IScheduler scheduler)
    {
        HaContext = haContext;
        Logger = logger;
        Notify = notify;
        Scheduler = scheduler;
        Entities = new Entities(haContext);
        Services = new Services(haContext);

        Logger.LogDebug("Started {Name}", GetType().Name);
    }
}