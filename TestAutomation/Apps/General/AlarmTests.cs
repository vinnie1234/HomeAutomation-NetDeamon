using Automation.apps.General;
using NetDaemon.Client;
using NSubstitute;
using TestAutomation.Helpers;
using Xunit;

namespace TestAutomation.Apps.General;

//todo
public class AlarmTests
{
    private readonly AppTestContext _ctx = AppTestContext.New();
    private readonly IHomeAssistantConnection _homeAssistantConnection = Substitute.For<IHomeAssistantConnection>();

    [Fact]
    public void ShouldSendNotificationWhenTravelTimeExceedsThreshold()
    {
        _ctx.InitApp<Alarm>(_homeAssistantConnection);
        _ctx.SetCurrentTime(DateTime.Parse("07:50:00"));
        _ctx.ChangeStateFor("sensor.here_travel_time_reistijd_in_het_verkeer")
            .FromState("30")
            .ToState("45");
    
        _ctx.VerifyCallNotify("notify", "mobile_app_phone_vincent");
    }

    [Fact]
    public void ShouldSendNotificationWhenTemperatureExceedsThreshold()
    {
        _ctx.InitApp<Alarm>(_homeAssistantConnection);
        _ctx.ChangeStateFor("sensor.badkamer_temperature")
            .FromState("20")
            .ToState("30");

        _ctx.VerifyCallNotify("notify", "mobile_app_phone_vincent");
    }

    [Fact]
    public void ShouldSendNotificationWhenEnergyConsumptionExceedsThreshold()
    {
        _ctx.InitApp<Alarm>(_homeAssistantConnection);
        _ctx.ChangeStateFor("sensor.p1_meter_3c39e72a64e8_active_power")
            .FromState("1500")
            .ToState("2500");

        _ctx.VerifyCallNotify("notify", "mobile_app_phone_vincent");
    }

    [Fact]
    public void ShouldSendNotificationWhenGarbageCollectionIsScheduled()
    {
        _ctx.InitApp<Alarm>(_homeAssistantConnection);
        _ctx.ChangeStateFor("sensor.afval_morgen")
            .FromState("Geen")
            .ToState("Papier");

        _ctx.VerifyCallNotify("notify", "mobile_app_phone_vincent");
    }

    [Fact]
    public void ShouldSendNotificationWhenPetSnowyErrorsDetected()
    {
        _ctx.InitApp<Alarm>(_homeAssistantConnection);
        _ctx.ChangeStateFor("sensor.petsnowy_litterbox_errors")
            .FromState("0")
            .ToState("1");

        _ctx.VerifyCallNotify("notify", "mobile_app_phone_vincent");
    }

    [Fact]
    public void ShouldSendNotificationWhenConnectionIsLost()
    {
        _ctx.InitApp<Alarm>(_homeAssistantConnection);
        _ctx.ChangeStateFor("sensor.ha_connection")
            .FromState("connected")
            .ToState("disconnected");

        _ctx.VerifyCallNotify("notify", "mobile_app_phone_vincent");
    }

    [Fact]
    public void ShouldSendNotificationWhenEnergyPriceIsNegative()
    {
        _ctx.InitApp<Alarm>(_homeAssistantConnection);
        _ctx.ChangeStateFor("sensor.energykwhnetpriceincents")
            .FromState("0")
            .ToState("-25.00");

        _ctx.VerifyCallNotify("notify", "mobile_app_phone_vincent");
    }

    [Fact]
    public void ShouldSendNotificationWhenNoRecentBackupsAreFound()
    {
        _ctx.InitApp<Alarm>(_homeAssistantConnection);;
        _ctx.SetAttributesFor("sensor.onedrivebackup", new { LastLocalbackupdate = DateTime.Now.AddDays(-3).ToString(), LastOneDrivebackupdate = DateTime.Now.AddDays(-3).ToString() });

        _ctx.VerifyCallNotify("notify", "notify_discord");
        _ctx.VerifyCallNotify("notify", "notify_discord");
    }
}