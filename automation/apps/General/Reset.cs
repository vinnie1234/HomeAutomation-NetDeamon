namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(Reset))]
public class Reset : BaseApp
{
    private readonly IDataRepository _storage;

    private List<LightStateModel>? LightEntitiesStates { get; set; }

    public Reset(IHaContext ha, ILogger<Reset> logger, IDataRepository storage, INotify notify, INetDaemonScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        _storage = storage;
        LightEntitiesStates = new List<LightStateModel>();

        if (Entities.InputBoolean.Disablereset.IsOff())
        {
            ResetAlarm();
            ResetLights();
        }
    }

    private void ResetAlarm()
    {
        //todo TTP steps!
        var oldAlarms = _storage.Get<List<AlarmStateModel>>("LightState");
        if (oldAlarms == null) return;

        var activeAlarmsHub = new List<AlarmStateModel>();
        var activeAlarmsHubJson = Entities.Sensor.HubVincentAlarms.Attributes?.Alarms;
        if (activeAlarmsHubJson != null)
            activeAlarmsHub.AddRange(from JsonElement o in activeAlarmsHubJson select o.Deserialize<AlarmStateModel>());


        var activeAlarmsLivingRoom = new List<AlarmStateModel>();
        var activeAlarmsLivingRoomJson = Entities.Sensor.WoonkamerAlarms.Attributes?.Alarms;
        if (activeAlarmsLivingRoomJson != null)
            activeAlarmsLivingRoom.AddRange(from JsonElement o in activeAlarmsLivingRoomJson
                select o.Deserialize<AlarmStateModel>());

        var allActiveAlarms = activeAlarmsHub.Concat(activeAlarmsLivingRoom);

        var activeAlarms = (List<AlarmStateModel>)allActiveAlarms;

        foreach (var alarm in activeAlarms
                     .Where(alarm => alarm.Status == "set")
                     .Where(alarm => oldAlarms
                         .All(x => x.AlarmId != alarm.AlarmId)))
        {
            if (alarm is { EntityId: not null, AlarmId: not null })
            {
                Notify.NotifyHouse(@$"Alarm van {alarm.LocalTime} word verwijderd");
                Services.GoogleHome.DeleteAlarm(alarm.EntityId, alarm.AlarmId);
            }
        }
    }

    private void ResetLights()
    {
        //todo TTP steps!
        LightEntitiesStates = _storage.Get<List<LightStateModel>>("LightState");

        Logger.LogDebug("Get {Count} light states", LightEntitiesStates?.Count);

        var properties = Entities.Light.GetType().GetProperties();

        foreach (var property in properties)
        {
            var light = (LightEntity)Entities.Light.GetType().GetProperty(property.Name)
                ?.GetValue(Entities.Light, null)!;

            var oldStateLight = LightEntitiesStates?.FirstOrDefault(x => x.EntityId == light.EntityId);

            if (oldStateLight != null)
            {
                switch (oldStateLight.IsOn)
                {
                    case false:
                        light.TurnOff();
                        break;
                    case true:
                        if ((light.Attributes?.SupportedColorModes ?? Array.Empty<string>()).Any(x => x == "xy"))
                        {
                            light.TurnOn(
                                rgbColor: oldStateLight.RgbColors,
                                brightness: Convert.ToInt64(oldStateLight.Brightness)
                            );
                        }
                        else
                        {
                            if (light.Attributes == null ||
                                (light.Attributes?.SupportedColorModes ?? Array.Empty<string>())
                                .Any(x => x == @"onoff"))
                            {
                                light.TurnOn();
                            }
                            else
                            {
                                light.TurnOn(
                                    colorTemp: oldStateLight.ColorTemp,
                                    brightness: Convert.ToInt64(oldStateLight.Brightness)
                                );
                            }
                        }

                        break;
                }
            }
        }
    }
}