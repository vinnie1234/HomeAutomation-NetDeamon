using System.Reactive.Concurrency;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(WaterManagement))]
public class WaterManagement : BaseApp
{
    private double? _waterUsages;
    private readonly IDataRepository _storage;
    
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public WaterManagement(
        IHaContext haContext, 
        ILogger<WaterManagement> logger, 
        INotify notify, 
        IScheduler scheduler, IDataRepository storage) 
        : base(haContext, logger, notify, scheduler)
    {
        _storage = storage;

        Entities.Sensor.Watermeter5c2faf0e9b0aTotalWaterUsage
            .StateChanges()
            .Subscribe(x =>
            {
                _waterUsages += (x.New?.State - x.Old?.State) * 1000;
            });

        Entities.Sensor.Watermeter5c2faf0e9b0aTotalWaterUsage
            .StateChanges()
            .Throttle(TimeSpan.FromSeconds(20))
            .Subscribe(_ =>
            {
                FindWaterUsage();
            });
    }

    private void FindWaterUsage()
    {
        Entities.InputSelect.WaterUsageSelector.SelectFirst();
        
        var guess = _waterUsages switch
        {
            < 5 => "WC Klein",
            > 5 and <= 8 => "WC Groot",
            > 8 and <= 30 => "Afwas",
            > 30 => "Douchen",
            _ => string.Empty
        };

        if (Entities.Sensor.WasmachinePower.State > 0) guess = "Wasmachine";

        Notify.NotifyPhoneVincent(@"Water Check",
            @$"Nieuw water gesignaleerd",
            true,
            action: new List<ActionModel>
            {
                new(action: "SendNotificationWater", title: $"Gok: {guess} : {_waterUsages}", func: () => { SaveWater(_waterUsages, guess); }),
                new(action: "SendNotificationWater", title: $"Anders : {_waterUsages}", func: () => { SaveWater(_waterUsages, guess); }),
                new(action: "SendNotificationWater", title: "Skip", func: () => { SaveWater(_waterUsages, "Skip"); }),
            });

        _waterUsages = 0;
    }

    private void SaveWater(double? value, string? guess)
    {
        if (guess != "Skip")
        {
            if (Entities.InputSelect.WaterUsageSelector.State != "Default") 
                guess = Entities.InputSelect.WaterUsageSelector.State;

            _storage.Save("WATERUSAGE", new
            {
                value,
                guess
            });
        }
    }
}