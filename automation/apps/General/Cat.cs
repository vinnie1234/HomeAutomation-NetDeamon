using Automation.Helpers;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(Cat))]
public class Cat : BaseApp
{
    public Cat(IHaContext haContext, ILogger<Cat> logger, INotify notify, INetDaemonScheduler scheduler)
        : base(haContext, logger, notify, scheduler)
    {
        Entities.InputButton.Feedcat.StateChanges()
            .Subscribe(_ =>
            {
                FeedCat(Convert.ToInt32(Entities.Sensor.ZedarLastAmountManualFeed.State));
                Entities.InputNumber.Zedarlastamountmanualfeed.SetValue(Convert.ToInt32(Entities.InputNumber.Zedarlastamountmanualfeed.State + Convert.ToInt32(Entities.Sensor.ZedarLastAmountManualFeed.State)));
                Entities.InputDatetime.Zedarlastmanualfeed.SetDatetime(new InputDatetimeSetDatetimeParameters
                {
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
            });

        Entities.InputButton.Zedargivenextfeedeary.StateChanges()
            .Subscribe(_ => GiveNextFeedEarly());

        Entities.InputButton.Cleanpetsnowy.StateChanges()
            .Subscribe(_ => CleanPetSnowy());        
        
        Entities.InputButton.Emptypetsnowy.StateChanges()
            .Subscribe(_ => EmptyPetSnowy());

        Entities.Sensor.PetsnowyStatus
            .StateChanges()
            .Subscribe(x =>
            {
                switch (x.New?.State)
                {
                    case "pet_into":
                        Entities.InputNumber.Pixelinpetsnowytime.SetValue(Convert.ToInt32(Entities.InputNumber.Pixelinpetsnowytime.State) + 1);
                        break;
                    case "cleaning":
                        Entities.InputNumber.Cleaningpetsnowytime.SetValue(Convert.ToInt32(Entities.InputNumber.Cleaningpetsnowytime.State) + 1);
                        break;
                }
            });

        AutoFeedCat();
        MonitorCar();
    }

    private void FeedCat(int amount)
    {
        Entities.InputNumber.Zedartotalamountfeedday.SetValue(Convert.ToInt32(Entities.InputNumber.Zedartotalamountfeedday.State + amount));
        Entities.InputNumber.Zedartotalamountfeedalltime.SetValue(Convert.ToInt32(Entities.InputNumber.Zedartotalamountfeedalltime.State + amount));
        
        Services.Localtuya.SetDp(new LocaltuyaSetDpParameters
        {
            DeviceId = ConfigManager.GetValueFromConfig(@"ZedarDeviceId"),
            Dp = 3,
            Value = amount
        });
    }

    private void MonitorCar()
    {
        Entities.InputDatetime.Zedarlastmanualfeed.StateChanges()
            .Subscribe(_ =>
                Notify.NotifyGsmVincent(@"Pixel heeft handmatig eten gehad",
                    @$"Pixel heeft {Entities.InputNumber.Zedarlastamountmanualfeed.State} porties eten gehad"));

        Entities.InputDatetime.Zedarlastautomatedfeed.StateChanges()
            .Subscribe(_ =>
                Notify.NotifyGsmVincent(@"Pixel heeft automatisch eten gehad",
                    @$"Pixel heeft {Entities.InputNumber.Zedarlastamountautomationfeed.State} porties eten gehad"));

        Scheduler.RunDaily(TimeSpan.Parse("23:59:58"), () => Entities.InputNumber.Zedartotalamountfeedday.SetValue(0));
    }

    private void AutoFeedCat()
    {
        foreach (var autoFeed in
                 Collections.GetFeedTimes(Entities).Where(autoFeed => autoFeed.Key.State != null))
        {
            Scheduler.RunDaily(TimeSpan.Parse(autoFeed.Key.State!), () =>
            {
                if (Entities.InputBoolean.Zedarskipnextautofeed.IsOff())
                {
                    FeedCat(Convert.ToInt32(autoFeed.Value.State));
                    
                    Entities.InputNumber.Zedarlastamountautomationfeed.SetValue(Convert.ToInt32(autoFeed.Value.State));
                    Entities.InputDatetime.Zedarlastautomatedfeed.SetDatetime(new InputDatetimeSetDatetimeParameters
                    {
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    });
                }
                
                Entities.InputBoolean.Zedarskipnextautofeed.TurnOff();
            });
        }
    }

    private void GiveNextFeedEarly()
    {
        var closestFeed = Collections.GetFeedTimes(Entities).MinBy(t =>
            Math.Abs((DateTime.Parse(t.Key.State!) - DateTime.Now).Ticks));

        Entities.InputBoolean.Zedarskipnextautofeed.TurnOn();
        FeedCat(Convert.ToInt32(closestFeed.Value.State));
        
        Entities.InputNumber.Zedarlastamountmanualfeed.SetValue(Convert.ToInt32(closestFeed.Value.State));

        Entities.InputDatetime.Zedarlastmanualfeed.SetDatetime(new InputDatetimeSetDatetimeParameters
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });
    }

    private void CleanPetSnowy()
    {
        Services.Localtuya.SetDp(new LocaltuyaSetDpParameters
        {
            DeviceId = ConfigManager.GetValueFromConfig(@"PetSnowyDeviceId"),
            Dp = 9,
            Value = "true"
        });
    }
    
    private void EmptyPetSnowy()
    {
        Services.Localtuya.SetDp(new LocaltuyaSetDpParameters
        {
            DeviceId = ConfigManager.GetValueFromConfig(@"PetSnowyDeviceId"),
            Dp = 109,
            Value = "true"
        });
    }
}