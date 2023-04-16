using NetDaemon.Extensions.Scheduler;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(Alarm))]
// ReSharper disable once UnusedType.Global
public class Alarm : BaseApp
{
    private readonly INetDaemonScheduler _scheduler;
    private bool IsSleeping => Entities.InputBoolean.Sleeping.IsOn();
    
    public Alarm(IHaContext ha, ILogger<Alarm> logger, INetDaemonScheduler scheduler, INotify notify)
        : base(ha, logger, notify)
    {
        _scheduler = scheduler;

        TemperatureCheck();
        EnergyCheck();
        GarbageCheck();

        Entities.BinarySensor.GangMotion.WhenTurnsOn(_ =>
        {
            if (AmIHomeCheck())
                Notify.NotifyGsmVincent("ALARM", "Motion detected", channel: "ALARM",
                    vibrationPattern: "100, 1000, 100, 1000, 100", ledColor: "red");
        });
    }

    private bool AmIHomeCheck()
    {
        return Entities.Person.VincentMaarschalkerweerd.State != "home";
    }

    private void TemperatureCheck()
    {
        Entities.Sensor.BadkamerTemperature
            .StateChanges()
            .Where(x => x.Entity.State > 25 && !IsSleeping)
            .Subscribe(x => Notify.NotifyGsmVincent("High Temperature detected",
                @$"Badkamer is {x.Entity.State} graden", channel: "ALARM",
                vibrationPattern: "100, 1000, 100, 1000, 100", ledColor: "red"));

        Entities.Sensor.BergingTemperature
            .StateChanges()
            .Where(x => x.Entity.State > 25 && !IsSleeping)
            .Subscribe(x => Notify.NotifyGsmVincent("High Temperature detected", @$"Berging is {x.Entity.State} graden",
                channel: "ALARM", vibrationPattern: "100, 1000, 100, 1000, 100", ledColor: "red"));

        Entities.Sensor.GangTemperature
            .StateChanges()
            .Where(x => x.Entity.State > 25 && !IsSleeping)
            .Subscribe(x => Notify.NotifyGsmVincent("High Temperature detected", @$"Gang is {x.Entity.State} graden",
                channel: "ALARM", vibrationPattern: "100, 1000, 100, 1000, 100", ledColor: "red"));
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
        _scheduler.RunDaily(TimeSpan.Parse("22:00:00"), () =>
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