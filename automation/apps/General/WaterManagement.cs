using System.Reactive.Concurrency;

namespace Automation.apps.General;

// [NetDaemonApp(Id = nameof(WaterManagement))]
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

        _waterUsages ??= 0;

        Entities.Sensor.Watermeter5c2faf0e9b0aTotalWaterUsage
            .StateChanges()
            .Subscribe(x =>
            {
                _waterUsages += (x.New?.State - x.Old?.State) * 1000;
                Entities.InputText.Lastwaterusageinliteractual.SetValue(_waterUsages.ToString() ?? string.Empty);
            });

        Entities.Sensor.Watermeter5c2faf0e9b0aTotalWaterUsage
            .StateChanges()
            .Throttle(TimeSpan.FromSeconds(20))
            .Subscribe(_ => { FindWaterUsage(); });
    }

    private void FindWaterUsage()
    {
        Entities.InputSelect.WaterUsageSelector.SelectFirst();

        if (_waterUsages != null)
        {
            var guess = Math.Round((double)_waterUsages) switch
            {
                <= 3          => "Kraan",
                >= 3 and <= 5 => "WC Klein",
                > 5 and <= 7  => "WC Groot",
                > 7 and <= 30 => "Afwas",
                > 30          => "Douchen",
                _             => "WC Klein"
            };

            if (Entities.Switch.Wasmachine.IsOn()) guess = "Wasmachine";
            var id = Guid.NewGuid();
            var waterUsage = _waterUsages;

            Notify.NotifyPhoneVincent($"Water Check ({_waterUsages})",
                "Nieuw water gesignaleerd",
                true,
                action: new List<ActionModel>
                {
                    new(action: "SendNotificationWaterGuess", title: $"Gok: {guess}",
                        func: () => { SaveWater(waterUsage.ToString()!, guess, id, DateTimeOffset.Now); }),
                    new(action: "SendNotificationWaterDifferent", title: "Anders",
                        func: () => { SaveWater(waterUsage.ToString()!, "", id, DateTimeOffset.Now); }),
                    new(action: "SendNotificationWaterSkip", title: "Skip",
                        func: () => { SaveWater(waterUsage.ToString()!, "Skip", id, DateTimeOffset.Now); })
                });

            _waterUsages = 0;
            Entities.InputText.Lastwaterusageinliteractual.SetValue(_waterUsages.ToString() ?? string.Empty);
        }
    }

    private void SaveWater(string value, string guess, Guid id, DateTimeOffset dateTime)
    {
        Entities.InputText.Lastwaterusageguess.SetValue(guess);

        if (guess != "Skip")
        {
            Entities.InputText.Lastwaterusageinlitersaved.SetValue(value);
            if (Entities.InputSelect.WaterUsageSelector.State != "Default")
                guess = Entities.InputSelect.WaterUsageSelector.State ?? string.Empty;

            if (!string.IsNullOrEmpty(guess))
            {
                var oldList =
                    _storage.Get<List<WaterSavingModel>>($"WATERUSAGE_{DateTimeOffset.Now.Year}_{DateTimeOffset.Now.Month}") ??
                    new List<WaterSavingModel>();

                if (oldList.Find(x => x.Id == id) != null) return;

                oldList.Add(new WaterSavingModel
                {
                    Id = id,
                    Value = value,
                    Guess = guess,
                    DateTime = dateTime
                });

                _storage.Save($"WATERUSAGE_{DateTimeOffset.Now.Year}_{DateTimeOffset.Now.Month}", oldList);
            }
        }
        else
        {
            Entities.InputText.Lastwaterusageinlitersaved.SetValue("0");
        }
    }
}