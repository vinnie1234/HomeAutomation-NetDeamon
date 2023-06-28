using System.Reactive.Concurrency;
using Automation.Helpers;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(Cat))]
public class Cat : BaseApp
{
    private readonly string _discordUri = ConfigManager.GetValueFromConfigNested("Discord", "Pixel") ?? "";

    public Cat(IHaContext haContext, ILogger<Cat> logger, INotify notify, IScheduler scheduler)
        : base(haContext, logger, notify, scheduler)
    {
        Entities.InputButton.Feedcat.StateChanges()
            .Subscribe(_ =>
            {
                FeedCat(Convert.ToInt32(Entities.InputNumber.Pixelnumberofmanualfood.State));
                Entities.InputNumber.Pixellastamountmanualfeed.SetValue(Convert.ToInt32(Entities.InputNumber.Pixelnumberofmanualfood.State + Convert.ToInt32(Entities.InputNumber.Pixellastamountmanualfeed.State)));
                Entities.InputDatetime.Pixellastmanualfeed.SetDatetime(new InputDatetimeSetDatetimeParameters
                {
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
            });

        Entities.InputButton.Pixelgivenextfeedeary.StateChanges()
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
        Entities.InputNumber.Pixeltotalamountfeedday.SetValue(Convert.ToInt32(Entities.InputNumber.Pixeltotalamountfeedday.State + amount));
        Entities.InputNumber.Pixeltotalamountfeedalltime.SetValue(Convert.ToInt32(Entities.InputNumber.Pixeltotalamountfeedalltime.State + amount));
        
        Services.Localtuya.SetDp(new LocaltuyaSetDpParameters
        {
            DeviceId = ConfigManager.GetValueFromConfig(@"SnowyFeeder"),
            Dp = 3,
            Value = amount
        });
        
        Logger.LogDebug(@"Dankjewel voor {Amount} porties of eten!", amount);
        Helpers.Discord.SendMessage(_discordUri, @$"Dankjewel voor {amount} porties of eten!");
    }

    private void MonitorCar()
    {
        Entities.InputDatetime.Pixellastmanualfeed.StateChanges()
            .Subscribe(_ =>
                Notify.NotifyPhoneVincent(@"Pixel heeft handmatig eten gehad",
                    @$"Pixel heeft {Entities.InputNumber.Pixellastamountmanualfeed.State} porties eten gehad", false, 5));

        Entities.InputDatetime.Pixellastautomatedfeed.StateChanges()
            .Subscribe(_ =>
            {
                Logger.LogDebug(@"NOTIFICATIE: Pixel heeft automatisch eten gehad");
                Notify.NotifyPhoneVincent(@"Pixel heeft automatisch eten gehad",
                    @$"Pixel heeft {Entities.InputNumber.Pixellastamountautomationfeed.State} porties eten gehad", false, 5);
            });
             

        Scheduler.ScheduleCron("59 23 * * *", () => Entities.InputNumber.Pixeltotalamountfeedday.SetValue(0));
    }

    private void AutoFeedCat()
    {
        foreach (var autoFeed in
                 Collections.GetFeedTimes(Entities).Where(autoFeed => autoFeed.Key.State != null))
        {
            Scheduler.RunDaily(TimeSpan.Parse(autoFeed.Key.State!), () =>
            {
                if (Entities.InputBoolean.Pixelskipnextautofeed.IsOff())
                {
                    FeedCat(Convert.ToInt32(autoFeed.Value.State));
                    
                    Entities.InputNumber.Pixellastamountautomationfeed.SetValue(Convert.ToInt32(autoFeed.Value.State));
                    Entities.InputDatetime.Pixellastautomatedfeed.SetDatetime(new InputDatetimeSetDatetimeParameters
                    {
                        Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                    });
                }
                
                Entities.InputBoolean.Pixelskipnextautofeed.TurnOff();
            });
        }
    }

    private void GiveNextFeedEarly()
    {
        var closestFeed = Collections.GetFeedTimes(Entities).MinBy(t =>
            Math.Abs((DateTime.Parse(t.Key.State!) - DateTime.Now).Ticks));

        Entities.InputBoolean.Pixelskipnextautofeed.TurnOn();
        FeedCat(Convert.ToInt32(closestFeed.Value.State));
        
        Entities.InputNumber.Pixellastamountmanualfeed.SetValue(Convert.ToInt32(closestFeed.Value.State));

        Entities.InputDatetime.Pixellastmanualfeed.SetDatetime(new InputDatetimeSetDatetimeParameters
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