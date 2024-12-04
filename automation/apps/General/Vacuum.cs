using System.Reactive.Concurrency;
using Automation.Helpers;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(Vacuum))]
public class Vacuum : BaseApp
{
    public Vacuum(
        IHaContext ha,
        ILogger<Alarm> logger,
        INotify notify,
        IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        CleanLitterBoxAfterUse();
        StartFromButton();
    }

    private void StartFromButton()
    {
        Entities.InputButton.Vacuumcleankattenbak.StateChanges().Subscribe(x =>
        {
            Clean("Kattenbak");
        });
        
        Entities.InputButton.Vacuumcleanbank.StateChanges().Subscribe(x =>
        {
            Clean("Bank");
        });
        
        Entities.InputButton.Vacuumcleangang.StateChanges().Subscribe(x =>
        {
            Clean("Gang");
        });
        
        Entities.InputButton.Vacuumcleanslaapkamer.StateChanges().Subscribe(x =>
        {
            Clean("Slaapkamer");
        });
        
        Entities.InputButton.Vacuumcleanwoonkamer.StateChanges().Subscribe(x =>
        {
            Clean("Woonkamer");
        });
    }

    private void CleanLitterBoxAfterUse()
    {
        Entities.Sensor.PetsnowyLitterboxStatus
            .StateChanges()
            .Where(x => x.New?.State == "cleaning")
            .Subscribe(_ =>
            {
                if (Entities.InputBoolean.Sleeping.IsOff())
                {
                    Clean("Kattenbak");
                }
            });
    }
    
    private void Clean(string cleanKey)
    {
        var zone = Collections.GetRoombaRooms().First(x => x.Key == cleanKey);

        Entities.Vacuum.Jaap.CallService("send_command",
            new
            {
                command = "start",
                @params = new
                {
                    pmap_id = ConfigManager.GetValueFromConfigNested("Roomba", "PmapId") ?? "",
                    regions = new[]
                    {
                        new
                        {
                            region_id = zone.Value.Item1,
                            type = zone.Value.Item2
                        }
                    }
                }
            });
    }
}