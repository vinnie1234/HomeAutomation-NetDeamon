namespace Automation.apps;

public class BaseApp
{
    internal readonly Entities Entities;
    internal readonly ILogger Logger;
    internal readonly INotify Notify;
    internal readonly IServices Services;
    internal readonly IHaContext HaContext;

    protected BaseApp(IHaContext haContext, ILogger logger, INotify notify)
    {
        HaContext = haContext;
        Logger = logger;
        Notify = notify;
        Entities = new Entities(haContext);
        Services = new Services(haContext);

        Logger.LogDebug("Started {Name}", GetType().Name);
    }
}