using Automation.Helpers;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(Alarm))]
public class Alarm : BaseApp
{
    private bool IsSleeping => Entities.InputBoolean.Sleeping.IsOn();
    
    public Alarm(IHaContext ha, ILogger<Alarm> logger, INotify notify, INetDaemonScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        TemperatureCheck();
        EnergyCheck();
        GarbageCheck();

        Entities.BinarySensor.GangMotion.WhenTurnsOn(_ =>
        {
            if (Globals.AmIHomeCheck(Entities))
                Notify.NotifyGsmVincent("ALARM", "Motion detected", channel: "ALARM",
                    vibrationPattern: "100, 1000, 100, 1000, 100", ledColor: "red");
        });
    }

    private void TemperatureCheck()
    {
        foreach (var temperatureSensor in Collections.GetAllTemperatureSensors(Entities))
        {
            temperatureSensor.Key
                .StateChanges()
                .Where(x => x.Entity.State > 25 && !IsSleeping)
                .Subscribe(x => Notify.NotifyGsmVincent("High Temperature detected",
                    @$"{temperatureSensor.Value} is {x.Entity.State} graden", channel: "ALARM",
                    vibrationPattern: "100, 1000, 100, 1000, 100", ledColor: "red"));
        }
    }

    private void EnergyCheck()
    {
        Entities.Sensor.P1Meter3c39e72a64e8ActivePower
            .StateChanges()
            .WhenStateIsFor(x => x?.State > 2000, TimeSpan.FromMinutes(10))
            .Subscribe(x =>
                {
                    Notify.NotifyGsmVincent(@"Hoog energie verbruik",
                        @$"Energie verbruik is al voor 10 minuten {x.Entity.State}",
                        new List<ActionModel>
                        {
                            new()
                            {
                                Action = "URI",
                                Title = @"Ga naar dashboard",
                                Uri = "https://vincent-huis.duckdns.org/energy"
                            }
                        },
                        channel: "ALARM", vibrationPattern: "100, 1000, 100, 1000, 100", ledColor: "red");
                }
            );
    }

    private void GarbageCheck()
    {
        Scheduler.RunDaily(TimeSpan.Parse("22:00:00"), () =>
        {
            var message = Entities.Sensor.AfvalMorgen.State;
            if (message != @"Geen")
            {
                Notify.NotifyGsmVincent(@"Vergeet het afval niet",
                    @$"Vergeet je niet op {message} buiten te zetten?");
            }
        });
    }

    // ReSharper disable once UnusedMember.Local
    private void HaChecks()
    {
        throw new NotImplementedException();
    }
}