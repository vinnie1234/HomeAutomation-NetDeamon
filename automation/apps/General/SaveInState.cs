namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(SaveInState))]
// ReSharper disable once UnusedType.Global
public class SaveInState : BaseApp
{
    private readonly IDataRepository _storage;

    public SaveInState(IHaContext ha, ILogger<SaveInState> logger, IDataRepository storage, INotify notify)
        : base(ha, logger, notify)
    {
        _storage = storage;
        LightEntitiesStates = new List<LightStateModel>();

        SetInitialStates();
    }

    private List<LightStateModel> LightEntitiesStates { get; }

    private void SetInitialStates()
    {
        var properties = Entities.Light.GetType().GetProperties();

        foreach (var property in properties)
        {
            var light = (LightEntity)Entities?.Light.GetType().GetProperty(property.Name)
                ?.GetValue(Entities.Light, null)!;
            SetLightEntityState(light);
        }

        _storage.Save("LightState", LightEntitiesStates);

        _storage.Save("AlarmState", SetAlarmState());

        Logger.LogDebug("Save state");
    }

    private IEnumerable<AlarmStateModel> SetAlarmState()
    {
        var activeAlarmsHub = new List<AlarmStateModel>();
        var activeAlarmsHubJson = Entities.Sensor.HubVincentAlarms.Attributes?.Alarms;
        if (activeAlarmsHubJson != null)
            activeAlarmsHub.AddRange(from JsonElement o in activeAlarmsHubJson select o.Deserialize<AlarmStateModel>());

        foreach (var alarmState in activeAlarmsHub)
        {
            alarmState.EntityId = Entities?.Sensor.HubVincentAlarms.EntityId;
        }

        var activeAlarmsLivingRoom = new List<AlarmStateModel>();
        var activeAlarmsLivingRoomJson = Entities?.Sensor.WoonkamerAlarms.Attributes?.Alarms;
        if (activeAlarmsLivingRoomJson != null)
            activeAlarmsLivingRoom.AddRange(from JsonElement o in activeAlarmsLivingRoomJson
                select o.Deserialize<AlarmStateModel>());

        foreach (var alarmState in activeAlarmsLivingRoom)
        {
            alarmState.EntityId = Entities?.Sensor.WoonkamerAlarms.EntityId;
        }

        return activeAlarmsHub.Concat(activeAlarmsLivingRoom);
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    private void SetLightEntityState(LightEntity entity)
    {
        var oldEntity = LightEntitiesStates.FirstOrDefault(x => x.EntityId == entity.EntityId);
        if (oldEntity != null)
        {
            LightEntitiesStates.Remove(oldEntity);
        }

        LightEntitiesStates.Add(new LightStateModel
        {
            EntityId = entity.EntityId,
            RgbColors = entity.Attributes?.RgbColor,
            Brightness = entity.Attributes?.Brightness,
            ColorTemp = entity.Attributes?.ColorTemp,
            IsOn = entity.IsOn(),
            SupportedColorModes = entity.Attributes?.SupportedColorModes
        });
    }
}