using System.Reactive.Concurrency;
using Automation.Helpers;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(Alarm))]
public class Alarm : BaseApp
{
    private bool IsSleeping => Entities.InputBoolean.Sleeping.IsOn();
    
    public Alarm(IHaContext ha, ILogger<Alarm> logger, INotify notify, IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        TemperatureCheck();
        EnergyCheck();
        GarbageCheck();
        PetSnowyCheck();

        Entities.BinarySensor.GangMotion.WhenTurnsOn(_ =>
        {
            if (Globals.AmIHomeCheck(Entities))
                Notify.NotifyPhoneVincent("ALARM", @"Beweging gedetecteerd", false, 5, channel: "ALARM",
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
                .Subscribe(x => Notify.NotifyPhoneVincent(@"Hoge temperatuur gedetecteerd",
                    @$"{temperatureSensor.Value} is {x.Entity.State} graden", true, channel: "ALARM",
                    vibrationPattern: "100, 1000, 100, 1000, 100", ledColor: "red"));
        }
    }

    private void EnergyCheck()
    {
        Entities.Sensor.P1Meter3c39e72a64e8ActivePower
            .StateChanges()
            .WhenStateIsFor(x => x?.State > 2000, TimeSpan.FromMinutes(10), Scheduler)
            .Subscribe(x =>
                {
                    Notify.NotifyPhoneVincent(@"Hoog energie verbruik",
                        @$"Energie verbruik is al voor 10 minuten {x.Entity.State}",
                        false,
                        10,
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
        Scheduler.ScheduleCron("00 22 * * *", () =>
        {
            var message = Entities.Sensor.AfvalMorgen.State;
            if (message != @"Geen")
                Notify.NotifyPhoneVincent(@"Vergeet het afval niet",
                    @$"Vergeet je niet op {message} buiten te zetten?", true);
        });
    }    
    
    private void PetSnowyCheck()
    {
        Scheduler.ScheduleCron("00 22 * * *", () =>
        {
            if (int.Parse(Entities.Sensor.PetsnowyError.State ?? "0") > 0)
                Notify.NotifyPhoneVincent(@"PetSnowy heeft errors",
                    @"Er staat nog een error open voor de PetSnowy", false, 10);
        });
    }

    // ReSharper disable once UnusedMember.Local
    private void HaChecks()
    {
        throw new NotImplementedException();
    }
}