using System.Reactive.Concurrency;
using Automation.Enum;

namespace Automation.Extensions;

public static class LightExtension
{
    public static void TurnAllOff(this LightEntities lightEntities)
    {
        lightEntities.EnumerateAll()
            .Where(x => x.EntityId is not "light.rt_ax88u_led" and "light.tradfri_driver")
            .TurnOff(transition: 5);
    }
    
    public static void TurnOnLightsWoonkamer(IEntities entities, IScheduler scheduler)
    {
        entities.Light.HueFilamentBulb2.TurnOn(brightnessPct: 100, kelvin: GetColorTemp(entities));
        entities.Light.HueFilamentBulb2
            .StateChanges()
            .Where(x => x.Old.IsOff())
            .Throttle(TimeSpan.FromMilliseconds(50))
            .Subscribe(_ => { entities.Light.PlafondWoonkamer.TurnOn(brightnessPct: 100, kelvin: GetColorTemp(entities)); });
        entities.Light.PlafondWoonkamer
            .StateChanges()
            .Where(x => x.Old.IsOff())
            .Throttle(TimeSpan.FromMilliseconds(50))
            .Subscribe(_ => { entities.Light.HueFilamentBulb1.TurnOn(brightnessPct: 100, kelvin: GetColorTemp(entities)); });

        scheduler.Schedule(TimeSpan.FromMilliseconds(200), () =>
        {
            entities.Light.HueFilamentBulb2.TurnOn(kelvin: GetColorTemp(entities));
            entities.Light.PlafondWoonkamer.TurnOn(kelvin: GetColorTemp(entities));
            entities.Light.HueFilamentBulb1.TurnOn(kelvin: GetColorTemp(entities));
        });
    }
    
    public static void TurnOffLightsWoonkamer(IEntities entities, IScheduler scheduler)
    {
        entities.Light.HueFilamentBulb1.TurnOff();
        entities.Light.HueFilamentBulb1
            .StateChanges()
            .Where(x => x.Old.IsOn())
            .Throttle(TimeSpan.FromMilliseconds(50))
            .Subscribe(_ => { entities.Light.PlafondWoonkamer.TurnOff(); });
        entities.Light.PlafondWoonkamer
            .StateChanges()
            .Where(x => x.Old.IsOn())
            .Throttle(TimeSpan.FromMilliseconds(50))
            .Subscribe(_ => { entities.Light.HueFilamentBulb2.TurnOff(); });

        scheduler.Schedule(TimeSpan.FromMilliseconds(200), () =>
        {
            entities.Light.HueFilamentBulb1.TurnOff();
            entities.Light.PlafondWoonkamer.TurnOff();
            entities.Light.HueFilamentBulb2.TurnOff();
        });
    }
    
    private static int GetColorTemp(IEntities entities)
    {
        var houseState = Globals.GetHouseState(entities);
        const int whiteColor = 4504;
        const int warmColor = 2300;
        const int someColor = 150;

        return houseState
            switch
            {
                HouseState.Day or HouseState.Morning   => whiteColor, // White color
                HouseState.Evening or HouseState.Night => warmColor,  // Warm Color
                _                                              => someColor   // Some Color
            };
    }
    
}