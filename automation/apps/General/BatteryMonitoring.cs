using System.Reactive.Concurrency;
using Automation.Helpers;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(BatteryMonitoring))]
public class BatteryMonitoring : BaseApp
{
    private const int BatteryWarningLevel = 20;


    public BatteryMonitoring(
        IHaContext ha, 
        ILogger<BatteryMonitoring> logger, 
        INotify notify,
        IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        foreach (var battySensor in Collections.GetAllBattySensors(Entities))
        {
            battySensor.Key
                .StateChanges()
                .WhenStateIsFor(x => x?.State is <= BatteryWarningLevel, TimeSpan.FromMinutes(10), Scheduler)
                .Subscribe(x => SendNotification(battySensor.Value, x.Entity.State ?? 0));
            
            battySensor.Key
                .StateChanges()
                .Where(x => x.Entity.State is 100)
                .Subscribe(_ => Notify.ResetNotificationHistoryForNotificationTitle(battySensor.Value));
        }
    }

    private void SendNotification(string name, double batterPrc)
    {
        Logger.LogDebug("Batterij bijna leeg van {Name}. De batterij is nu op {BatterPrc}", name, batterPrc);
        Notify.NotifyPhoneVincent(
            $"Batterij bijna leeg van {name}",
            $"Het is tijd om de batterij op te laden van {name}. De batterij is nu op {batterPrc}%",
            false,
            TimeSpan.FromDays(7).Minutes,
            new List<ActionModel>
            {
                new(action: "URI", title: "Ga naar batterij checks",
                    uri: ConfigManager.GetValueFromConfig("BaseUrlHomeAssistant") + "/status-huis")
            });
    }
}