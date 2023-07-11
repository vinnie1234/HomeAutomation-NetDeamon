using System.Reactive.Concurrency;

namespace Automation.apps.General;

public class WaterManagement : BaseApp
{
    private long _waterUsages = 0;
    
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    protected WaterManagement(
        IHaContext haContext, 
        ILogger<WaterManagement> logger, 
        INotify notify, 
        IScheduler scheduler) 
        : base(haContext, logger, notify, scheduler)
    {
        Entities.Sensor.Watermeter5c2faf0e9b0aTotalWaterUsage
            .StateChanges()
            .Subscribe(x =>
            {
                _waterUsages = Convert.ToInt64((x.New?.State - x.Old?.State) / 1000);
            });

        Entities.Sensor.Watermeter5c2faf0e9b0aTotalWaterUsage
            .StateChanges()
            .WhenStateIsFor(x => x?.LastChanged < DateTime.Now.AddSeconds(10), TimeSpan.FromMinutes(1), Scheduler)
            .Subscribe(_ =>
            {
                FindWaterUsage();
            });
    }

    private void FindWaterUsage()
    {
        //todo find smart way
    }
}