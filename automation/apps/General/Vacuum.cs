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
        var buttons = new Dictionary<InputButtonEntity, string>
        {
            {
                Entities.InputButton.Vacuumcleankattenbak, "Kattenbak"
            },
            {
                Entities.InputButton.Vacuumcleanbank, "Bank"
            },
            {
                Entities.InputButton.Vacuumcleangang, "Gang"
            },
            {
                Entities.InputButton.Vacuumcleanslaapkamer, "Slaapkamer"
            },
            {
                Entities.InputButton.Vacuumcleanwoonkamer, "Woonkamer"
            }

        };
        
        foreach (var button in buttons)
        {
            button.Key.StateChanges().Subscribe(x =>
            {
                Clean(button.Value);
            });
        }
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
                else
                {
                    Entities.InputBoolean.Sleeping
                        .StateChanges()
                        .Where(x => x.New.IsOff()).Subscribe(_ =>
                        {
                            Clean("Kattenbak");
                        });
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