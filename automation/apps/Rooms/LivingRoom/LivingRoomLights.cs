using System.Reactive.Concurrency;
using Automation.Enum;

namespace Automation.apps.Rooms.LivingRoom;

[NetDaemonApp(Id = nameof(LivingRoomLights))]
public class LivingRoomLights : BaseApp
{
    public LivingRoomLights(IHaContext ha, ILogger<LivingRoomLights> logger, INotify notify, IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        HaContext.Events.Where(x => x.EventType == "hue_event").Subscribe(x =>
        {
            var eventModel = x.DataElement?.ToObject<EventModel>();
            if (eventModel != null) TurnOnPlafond(eventModel);
        });
    }

    // ReSharper disable once IdentifierTypo
    private void TurnOnPlafond(EventModel eventModel)
    {
        if (eventModel is { DeviceId: "b4784a8e43cc6f5aabfb6895f3a8dbac", Type: "initial_press" })
        {
            if (Entities.Light.HueFilamentBulb2.IsOff())
            {
                Entities.Light.HueFilamentBulb2.TurnOn(brightnessPct: 100, kelvin: GetColorTemp());
                Entities.Light.HueFilamentBulb2
                    .StateChanges()
                    .Where(x => x.Old.IsOff())
                    .Throttle(TimeSpan.FromMilliseconds(50))
                    .Subscribe(_ => { Entities.Light.PlafondWoonkamer.TurnOn(brightnessPct: 100, kelvin: GetColorTemp()); });
                Entities.Light.PlafondWoonkamer
                    .StateChanges()
                    .Where(x => x.Old.IsOff())
                    .Throttle(TimeSpan.FromMilliseconds(50))
                    .Subscribe(_ => { Entities.Light.HueFilamentBulb1.TurnOn(brightnessPct: 100, kelvin: GetColorTemp()); });
                
                Scheduler.Schedule(TimeSpan.FromMilliseconds(200), () =>
                {
                    Entities.Light.HueFilamentBulb2.TurnOn(); 
                    Entities.Light.PlafondWoonkamer.TurnOn(); 
                    Entities.Light.HueFilamentBulb1.TurnOn();
                });
            }
            else
            {
                Entities.Light.HueFilamentBulb1.TurnOff();
                Entities.Light.HueFilamentBulb1
                    .StateChanges()
                    .Where(x => x.Old.IsOn())
                    .Throttle(TimeSpan.FromMilliseconds(50))
                    .Subscribe(_ => { Entities.Light.PlafondWoonkamer.TurnOff(); });
                Entities.Light.PlafondWoonkamer
                    .StateChanges()
                    .Where(x => x.Old.IsOn())
                    .Throttle(TimeSpan.FromMilliseconds(50))
                    .Subscribe(_ => { Entities.Light.HueFilamentBulb2.TurnOff(); });

                Scheduler.Schedule(TimeSpan.FromMilliseconds(200), () =>
                {
                    Entities.Light.HueFilamentBulb1.TurnOff();
                    Entities.Light.PlafondWoonkamer.TurnOff();
                    Entities.Light.HueFilamentBulb2.TurnOff();
                });
            }
        }
    }

    private int GetColorTemp()
    {
        var houseState = Globals.GetHouseState(Entities);

        return houseState switch
        {
            HouseStateEnum.Day or HouseStateEnum.Morning   => 4504,
            HouseStateEnum.Evening or HouseStateEnum.Night => 2300,
            _                                              => 150
        };
    }
}