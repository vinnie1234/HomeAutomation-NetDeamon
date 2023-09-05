using System.Reactive.Concurrency;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(SaveInState))]
public class SaveInState : BaseApp
{
    private readonly IDataRepository _storage;
    private List<LightStateModel> LightEntitiesStates { get; }

    public SaveInState(
        IHaContext ha,
        ILogger<SaveInState> logger,
        IDataRepository storage,
        INotify notify,
        IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        _storage = storage;
        LightEntitiesStates = new List<LightStateModel>();

        SetInitialStates();
    }

    private void SetInitialStates()
    {
        var properties = Entities.Light.GetType().GetProperties();

        foreach (var property in properties)
        {
            var light = property.GetValue(Entities.Light, null);
            if (light != null)
                SetLightEntityState((LightEntity)light);
        }

        _storage.Save("LightState", LightEntitiesStates);

        _storage.Save("AlarmState", SetAlarmState());

        Logger.LogDebug("Save state");
    }

    private IEnumerable<AlarmStateModel?> SetAlarmState()
    {
        var activeAlarmsHub = new List<AlarmStateModel?>();
        var activeAlarmsHubJson = Entities.Sensor.HubVincentAlarms.Attributes?.Alarms;
        if (activeAlarmsHubJson != null)
            activeAlarmsHub
                .AddRange(activeAlarmsHubJson.Cast<JsonElement>().Select(o => o.Deserialize<AlarmStateModel>()));

        foreach (var alarmState in activeAlarmsHub.Where(alarmState => alarmState != null))
            if (alarmState != null) 
                alarmState.EntityId = Entities?.Sensor.HubVincentAlarms.EntityId;

        return activeAlarmsHub;
    }

    // ReSharper disable once SuggestBaseTypeForParameter
    private void SetLightEntityState(LightEntity entity)
    {
        var oldEntity = LightEntitiesStates
            .FirstOrDefault(lightStateModel => lightStateModel.EntityId == entity.EntityId);
        if (oldEntity != null) LightEntitiesStates.Remove(oldEntity);

        LightEntitiesStates.Add(new LightStateModel(entityId: entity.EntityId, rgbColors: entity.Attributes?.RgbColor,
            brightness: entity.Attributes?.Brightness, colorTemp: entity.Attributes?.ColorTemp, isOn: entity.IsOn(),
            supportedColorModes: entity.Attributes?.SupportedColorModes));
    }
}