namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(BatteryMonitoring))]
public class BatteryMonitoring : BaseApp
{
    private const int BatteryWarningLevel = 20;

    public BatteryMonitoring(IHaContext ha, ILogger<BatteryMonitoring> logger, INotify notify,
        INetDaemonScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        foreach (var battySensor in GetAllBattySensors())
        {
            battySensor.Key
                .StateChanges()
                .Where(x => x.Entity.State is <= BatteryWarningLevel)
                .Subscribe(x => SendNotification(battySensor.Value, x.Entity.State));
        }

        //todo temp disabled
        /*Entities.Sensor.JaapBatteryLevel
            .StateChanges()
            .Where(x => x.Entity.State is <= BatteryWarningLevel)
            .Subscribe(x => SendNotification(@"Jaap", x.Entity.State)); */
    }

    private void SendNotification(string name, double? batterPrc)
    {
        Logger.LogDebug(@"Batterij bijna leeg van {Name}. De batterij is nu op {BatterPrc}", name, batterPrc);
        Notify.NotifyGsmVincent(
            @$"Batterij bijna leeg van {name}",
            @$"Het is tijd om de batterij op te laden van {name}. De batterij is nu op {batterPrc}%",
            new List<ActionModel>
            {
                new()
                {
                    Action = "URI",
                    Title = @"Ga naar batterij checks",
                    Uri = "https://vincent-huis.duckdns.org/status-huis"
                }
            });
    }

    private Dictionary<NumericSensorEntity, string> GetAllBattySensors()
    {
        return new Dictionary<NumericSensorEntity, string>
        {
            { Entities.Sensor.BadkamerBattery, @"Wall switch Badkamer" },
            { Entities.Sensor.BadkamerBattery2, @"Hue switch Badkamer" },
            { Entities.Sensor.BadkamermotionBattery, @"Motion Detector Badkamer" },
            { Entities.Sensor.SwitchBadkamerSpiegelBattery, @"Hue switch Badkamerspiegel" },
            { Entities.Sensor.GangBattery, "Motion Detector Gang" },
            { Entities.Sensor.GangBattery2, "Hue switch switch Gang" },
            { Entities.Sensor.HalBattery, "Wall switch switch Gang" },
            { Entities.Sensor.BergingBattery, @"Motion Detector Berging" },
            { Entities.Sensor.WoonkamerBattery, @"Hue switch woonkamer" },
            { Entities.Sensor.WoonkamerBattery2, @"Wall switch Woonkamer" },
            { Entities.Sensor.SlaapkamerBattery, @"Wall switch Slaapkamer" },
            { Entities.Sensor.SwitchBadkamerSpiegelBattery, @"Wall switch Badkamer" },
            { Entities.Sensor.Rollerblind0001Battery, @"Rolluik Slaapkamer" },
            { Entities.Sensor.BotA801Battery, @"Switchbot" },
            { Entities.Sensor.KeukenAfstandbediening, @"Keuken afstandbediening" },
            //{Entities.Sensor.JaapBatteryLevel, "Jaap"},
        };
    }
}