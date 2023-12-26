using System.Globalization;
using System.Reactive.Concurrency;
using Automation.Helpers;
using Automation.Models.DiscordNotificationModels;

namespace Automation.apps.General;

/*
[NetDaemonApp(Id = nameof(Cat))]
public class Cat : BaseApp
{
    private readonly string _discordPixelChannel = ConfigManager.GetValueFromConfigNested("Discord", "Pixel") ?? "";

    public Cat(
        IHaContext haContext,
        ILogger<Cat> logger,
        INotify notify,
        IScheduler scheduler)
        : base(haContext, logger, notify, scheduler)
    {
        ButtonFeedCat();
        PetSnowyStatusMonitoring();
        AutoFeedCat();
        MonitorCar();

        Entities.InputButton.Pixelgivenextfeedeary.StateChanges()
            .Subscribe(_ => GiveNextFeedEarly());

        Entities.InputButton.Cleanpetsnowy.StateChanges()
            .Subscribe(_ => CleanPetSnowy());

        Entities.InputButton.Emptypetsnowy.StateChanges()
            .Subscribe(_ => EmptyPetSnowy());
    }

    private void PetSnowyStatusMonitoring()
    {
        Entities.Sensor.PetsnowyStatus
            .StateChanges()
            .Subscribe(x =>
            {
                switch (x.New?.State)
                {
                    case "pet_into":
                        Entities.InputNumber.Pixelinpetsnowytime.SetValue(
                            Convert.ToInt32(Entities.InputNumber.Pixelinpetsnowytime.State) + 1);
                        break;
                    case "cleaning":
                        Entities.InputNumber.Cleaningpetsnowytime.SetValue(
                            Convert.ToInt32(Entities.InputNumber.Cleaningpetsnowytime.State) + 1);
                        break;
                }
            });
    }

    private void ButtonFeedCat()
    {
        Entities.InputButton.Feedcat.StateChanges()
            .Subscribe(_ =>
            {
                //Cause inputNumber is always an double and for Tuya need an int the double will convert to int
                FeedCat(Convert.ToInt32(Entities.InputNumber.Pixelnumberofmanualfood.State ?? 0.0));
                Entities.InputNumber.Pixellastamountmanualfeed.SetValue(Convert.ToInt32(
                    Entities.InputNumber.Pixelnumberofmanualfood.State +
                    Convert.ToInt32(Entities.InputNumber.Pixellastamountmanualfeed.State ?? 0.0)));
                Entities.InputDatetime.Pixellastmanualfeed.SetDatetime(new InputDatetimeSetDatetimeParameters
                {
                    Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                });
            });
    }

    private void FeedCat(int amount)
    {
        var amountToday = Convert.ToInt32(Entities.InputNumber.Pixeltotalamountfeedday.State + amount ?? 0);
        Entities.InputNumber.Pixeltotalamountfeedday.SetValue(amountToday);
        Entities.InputNumber.Pixeltotalamountfeedalltime.SetValue(
            Convert.ToInt32(Entities.InputNumber.Pixeltotalamountfeedalltime.State + amount ?? 0));

        Services.Localtuya.SetDp(new LocaltuyaSetDpParameters
        {
            DeviceId = ConfigManager.GetValueFromConfig(@"SnowyFeeder"),
            Dp = 3,
            Value = amount
        });

        Logger.LogDebug(@"Dankjewel voor {Amount} porties of eten!", amount);

        var discordNotificationModel = new DiscordNotificationModel
        {
            Embed = new Embed
            {
                Title = @"Pixel heeft eten gehad",
                Url = ConfigManager.GetValueFromConfig("BaseUrlHomeAssistant") + @"/dwains-dashboard/more_page_cat_blue_print",
                Thumbnail = new Location("https://cdn.pixabay.com/photo/2016/10/11/18/17/black-cat-1732366_960_720.png"),
                Fields = new[]
                {
                    new Field { Name = @"Eten gegeven", Value = amount.ToString() },
                    new Field { Name = @"Totaal gehad vandaag", Value = amountToday.ToString() }
                }
            }
        };

        Notify.NotifyDiscord("", new[] { _discordPixelChannel }, discordNotificationModel);
    }

    private void MonitorCar()
    {
        Entities.InputDatetime.Pixellastmanualfeed.StateChanges()
            .Subscribe(_ =>
                Notify.NotifyPhoneVincent(@"Pixel heeft handmatig eten gehad",
                    @$"Pixel heeft {Entities.InputNumber.Pixellastamountmanualfeed.State ?? 0} porties eten gehad",
                    false, 5));

        Entities.InputDatetime.Pixellastautomatedfeed.StateChanges()
            .Subscribe(_ =>
            {
                Logger.LogDebug(@"NOTIFICATIE: Pixel heeft automatisch eten gehad");
                Notify.NotifyPhoneVincent(@"Pixel heeft automatisch eten gehad",
                    @$"Pixel heeft {Entities.InputNumber.Pixellastamountautomationfeed.State ?? 0} porties eten gehad",
                    false, 5);
            });


        Scheduler.ScheduleCron("59 23 * * *", () => Entities.InputNumber.Pixeltotalamountfeedday.SetValue(0));
    }

    private void AutoFeedCat()
    {
        foreach (var autoFeed in
                 Collections.GetFeedTimes(Entities).Where(autoFeed => autoFeed.Key.State != null))
            Scheduler.RunDaily(TimeSpan.Parse(autoFeed.Key.State!, new CultureInfo("nl-Nl")), () =>
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

    private void GiveNextFeedEarly()
    {
        var closestFeed = GetClosestFeed();

        Entities.InputBoolean.Pixelskipnextautofeed.TurnOn();
        FeedCat(Convert.ToInt32(closestFeed.Value.State));

        Entities.InputNumber.Pixellastamountmanualfeed.SetValue(Convert.ToInt32(closestFeed.Value.State));

        Entities.InputDatetime.Pixellastmanualfeed.SetDatetime(new InputDatetimeSetDatetimeParameters
        {
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });
    }

    private KeyValuePair<InputDatetimeEntity, InputNumberEntity> GetClosestFeed()
    {
        var closestFeed =
            Collections
                .GetFeedTimes(Entities)
                .MinBy(pair =>
                    Math.Abs(
                        (DateTimeOffset.Parse(pair.Key.State ?? throw new InvalidOperationException(), new CultureInfo("nl-Nl")) - DateTimeOffset.Now)
                        .Ticks));
        return closestFeed;
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
} */ //todo