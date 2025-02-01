using Automation.apps.General;
using TestAutomation.Helpers;
using Xunit;

namespace TestAutomation.Apps.General;

public class AwayManagerTests
{
    private readonly AppTestContext _ctx = AppTestContext.New();

    [Fact]
    public void ShouldTurnOffAwayWhenVincentComesHome()
    {
        _ctx.InitApp<AwayManager>();
        _ctx.ChangeStateFor("input_boolean.away")
            .FromState("off")
            .ToState("on");
        _ctx.ChangeStateFor("person.vincent_maarschalkerweerd")
            .FromState("")
            .ToState("home");        
        
        _ctx.VerifyCallService("input_boolean", "turn_off", "away");
    }

    [Fact]
    public void ShouldSendNotificationWhenAwayStateIsActivated()
    {
        _ctx.InitApp<AwayManager>();
        _ctx.ChangeStateFor("input_boolean.away")
            .FromState("Off")
            .ToState("On");

        _ctx.VerifyCallNotify("notify", "notify_phone_vincent");
    }

    [Fact]
    public void ShouldTurnOffAllLightsWhenAwayStateIsActivated()
    {
        _ctx.InitApp<AwayManager>();
        _ctx.ChangeStateFor("input_boolean.away")
            .FromState("off")
            .ToState("on");

        _ctx.VerifyCallService("light", "turn_off", "");
    }

    [Fact]
    public void ShouldSendWelcomeHomeNotificationWhenVincentComesHome()
    {
        _ctx.InitApp<AwayManager>();
        _ctx.ChangeStateFor("binary_sensor.gang_motion")
            .FromState("off")
            .ToState("on");

        _ctx.VerifyCallNotify("notify", "notify_house");
    }

    [Fact]
    public void ShouldSetCorrectLightSceneBasedOnHouseState()
    {
        _ctx.InitApp<AwayManager>();
        _ctx.ChangeStateFor("sensor.house_state")
            .FromState("")
            .ToState("morning");

        _ctx.VerifyCallService("scene", "turn_on", "scene.woonkamermorning");
    }

    // [Fact]
    // public void ShouldActivateAwayStateWhenVincentIsFarAwayForLong()
    // {
    //     _ctx.InitApp<AwayManager>();
    //     _ctx.ChangeStateFor("sensor.thuis_phone_vincent_distance")
    //         .FromState("200")
    //         .ToState("350");
    //
    //     _ctx.VerifyStateChange("input_boolean", "turn_on", "away", "on");
    // }
}