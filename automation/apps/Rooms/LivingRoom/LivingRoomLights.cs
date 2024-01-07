using System.Reactive.Concurrency;

namespace Automation.apps.Rooms.LivingRoom;

[NetDaemonApp(Id = nameof(LivingRoomLights))]
public class LivingRoomLights : BaseApp
{
    public LivingRoomLights(
        IHaContext ha,
        ILogger<LivingRoomLights> logger,
        INotify notify,
        IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        HaContext.Events.Where(x => x.EventType == "hue_event").Subscribe(x =>
        {
            var eventModel = x.DataElement?.ToObject<EventModel>();
            if (eventModel != null) TurnOnPlafond(eventModel);
        });

        Entities.InputSelect.Housemodeselect
            .StateChanges()
            .Where(_ => Entities.Light.HueFilamentBulb2.IsOn())
            .Subscribe(_ =>
            {
                LightExtension.TurnOnLightsWoonkamer(Entities, Scheduler);
            });

    }

    // ReSharper disable once IdentifierTypo
    private void TurnOnPlafond(EventModel eventModel)
    {
        const string hueWallLivingRoomId = "b4784a8e43cc6f5aabfb6895f3a8dbac";

        if (eventModel is { DeviceId: hueWallLivingRoomId, Type: "initial_press" })
        {
            if (Entities.Light.HueFilamentBulb2.IsOff())
            {
                LightExtension.TurnOnLightsWoonkamer(Entities, Scheduler);
            }
            else
            {
                LightExtension.TurnOffLightsWoonkamer(Entities, Scheduler);
            }
        }
    }
}