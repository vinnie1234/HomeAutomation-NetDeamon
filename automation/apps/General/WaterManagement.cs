namespace Automation.apps.General;

public class WaterManagement : BaseApp
{
    private long _waterUsages = 0;
    
    protected WaterManagement(IHaContext haContext, ILogger logger, INotify notify, INetDaemonScheduler scheduler) : base(haContext, logger, notify, scheduler)
    {
        Entities.Sensor.Watermeter5c2faf0e9b0aTotalWaterUsage
            .StateChanges()
            .Subscribe(x =>
            {
                _waterUsages = Convert.ToInt64((x.New?.State - x.Old?.State) / 1000);
            });

        Entities.Sensor.Watermeter5c2faf0e9b0aTotalWaterUsage
            .StateChanges()
            .WhenStateIsFor(x => x?.LastChanged < DateTime.Now.AddSeconds(10), TimeSpan.FromMinutes(1))
            .Subscribe(x =>
            {
                FindWaterUsage();
            });
    }

    private void FindWaterUsage()
    {
        //todo find smart way
    }
}