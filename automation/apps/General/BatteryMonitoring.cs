namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(BatteryMonitoring))]
// ReSharper disable once UnusedType.Global
public class BatteryMonitoring : BaseApp
{
    private const int BatteryWarningLevel = 20;

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public BatteryMonitoring(IHaContext ha, ILogger<BatteryMonitoring> logger, INotify notify)
        : base(ha, logger, notify)
    {
        Entities.Sensor.BadkamerBattery
            .StateChanges()
            .Where(x => x.Entity.State is <= BatteryWarningLevel)
            .Subscribe(x => SendNotification(@"Wall switch Badkamer", x.Entity.State));

        Entities.Sensor.BadkamerBattery2
            .StateChanges()
            .Where(x => x.Entity.State is <= BatteryWarningLevel)
            .Subscribe(x => SendNotification(@"Hue switch Badkamer", x.Entity.State));

        Entities.Sensor.BadkamermotionBattery
            .StateChanges()
            .Where(x => x.Entity.State is <= BatteryWarningLevel)
            .Subscribe(x => SendNotification(@"Motion Detector Badkamer", x.Entity.State));

        Entities.Sensor.SwitchBadkamerSpiegelBattery
            .StateChanges()
            .Where(x => x.Entity.State is <= BatteryWarningLevel)
            .Subscribe(x => SendNotification(@"Hue switch Badkamerspiegel", x.Entity.State));

        Entities.Sensor.GangBattery
            .StateChanges()
            .Where(x => x.Entity.State is <= BatteryWarningLevel)
            .Subscribe(x => SendNotification(@"Motion Detector Gang", x.Entity.State));

        Entities.Sensor.GangBattery2
            .StateChanges()
            .Where(x => x.Entity.State is <= BatteryWarningLevel)
            .Subscribe(x => SendNotification(@"Hue switch switch Gang", x.Entity.State));

        Entities.Sensor.HalBattery
            .StateChanges()
            .Where(x => x.Entity.State is <= BatteryWarningLevel)
            .Subscribe(x => SendNotification(@"Wall switch switch Gang", x.Entity.State));

        Entities.Sensor.BergingBattery
            .StateChanges()
            .Where(x => x.Entity.State is <= BatteryWarningLevel)
            .Subscribe(x => SendNotification(@"Motion Detector Berging", x.Entity.State));

        Entities.Sensor.WoonkamerBattery
            .StateChanges()
            .Where(x => x.Entity.State is <= BatteryWarningLevel)
            .Subscribe(x => SendNotification(@"Hue switch woonkamer", x.Entity.State));

        Entities.Sensor.WoonkamerBattery2
            .StateChanges()
            .Where(x => x.Entity.State is <= BatteryWarningLevel)
            .Subscribe(x => SendNotification(@"Wall switch Woonkamer", x.Entity.State));

        Entities.Sensor.SlaapkamerBattery
            .StateChanges()
            .Where(x => x.Entity.State is <= BatteryWarningLevel)
            .Subscribe(x => SendNotification(@"Wall switch Slaapkamer", x.Entity.State));

        Entities.Sensor.SwitchBadkamerSpiegelBattery
            .StateChanges()
            .Where(x => x.Entity.State is <= BatteryWarningLevel)
            .Subscribe(x => SendNotification(@"Wall switch Badkamer", x.Entity.State));

        Entities.Sensor.Rollerblind0001Battery
            .StateChanges()
            .Where(x => x.Entity.State is <= BatteryWarningLevel)
            .Subscribe(x => SendNotification(@"Rolluik Slaapkamer", x.Entity.State));

        Entities.Sensor.BotA801Battery
            .StateChanges()
            .Where(x => x.Entity.State is <= BatteryWarningLevel)
            .Subscribe(x => SendNotification(@"Switchbot", x.Entity.State));

        Entities.Sensor.JaapBatteryLevel
            .StateChanges()
            .Where(x => x.Entity.State is <= BatteryWarningLevel)
            .Subscribe(x => SendNotification(@"Jaap", x.Entity.State));
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
}