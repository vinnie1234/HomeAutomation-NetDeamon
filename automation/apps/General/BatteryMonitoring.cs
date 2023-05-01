using Automation.Helpers;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(BatteryMonitoring))]
public class BatteryMonitoring : BaseApp
{
    private const int BatteryWarningLevel = 20;

    public BatteryMonitoring(IHaContext ha, ILogger<BatteryMonitoring> logger, INotify notify,
        INetDaemonScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        foreach (var battySensor in Collections.GetAllBattySensors(Entities))
        {
            battySensor.Key
                .StateChanges()
                .Where(x => x.Entity.State is <= BatteryWarningLevel)
                .Subscribe(x => SendNotification(battySensor.Value, x.Entity.State));
        }
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