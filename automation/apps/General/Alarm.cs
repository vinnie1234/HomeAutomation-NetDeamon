using System.Reactive.Concurrency;
using Automation.Helpers;
using NetDaemon.Client;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(Alarm))]
public class Alarm : BaseApp
{
    private bool IsSleeping => Entities.InputBoolean.Sleeping.IsOn();
    private readonly string _discordUri = ConfigManager.GetValueFromConfigNested("Discord", "Logs") ?? "";
    
    public Alarm(
        IHaContext ha, 
        ILogger<Alarm> logger, 
        INotify notify, 
        IScheduler scheduler, 
        IHomeAssistantConnection homeAssistantConnection)
        : base(ha, logger, notify, scheduler)
    {
        TemperatureCheck();
        EnergyCheck();
        GarbageCheck();
        PetSnowyCheck();
        HaChecks(homeAssistantConnection);
        EnergyNegativeCheck();

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
            temperatureSensor.Key
                .StateChanges()
                .Where(x => x.Entity.State > 25 && !IsSleeping)
                .Subscribe(x => Notify.NotifyPhoneVincent(@"Hoge temperatuur gedetecteerd",
                    @$"{temperatureSensor.Value} is {x.Entity.State} graden", true, channel: "ALARM",
                    vibrationPattern: "100, 1000, 100, 1000, 100", ledColor: "red"));
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
                            new(action: "URI", title: @"Ga naar dashboard",
                                uri: ConfigManager.GetValueFromConfig("BaseUrlHomeAssistant") + "/energy")
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
            {
                Helpers.Discord.SendMessage(_discordUri, @"PetSnowy heeft errors");
                Notify.NotifyPhoneVincent(@"PetSnowy heeft errors",
                    @"Er staat nog een error open voor de PetSnowy", false, 10);
            }
        });
    }
    
    private void HaChecks(IHomeAssistantConnection homeAssistantConnection)
    {
        /*Scheduler.RunEvery(TimeSpan.FromSeconds(30), DateTimeOffset.Now, () =>
        {
            var ping = homeAssistantConnection.PingAsync(TimeSpan.FromSeconds(5), new CancellationToken()).Result;
            if (!ping)
            {
                Helpers.Discord.SendMessage(_discordUri, @"NetDeamon heeft geen verbinding meer met HA");
                Notify.NotifyPhoneVincent(@"NetDeamon heeft geen verbinding meer met HA",
                    @"De ping naar HA is helaas niet gelukt!", false, 10);
            }
        }); */
    }

    private void EnergyNegativeCheck()
    {
        Entities.Sensor.Energykwhnetpriceincents
            .StateChanges()
            .Subscribe(x =>
            {
                if (x.New?.State < 20.00)
                {
                    Helpers.Discord.SendMessage(_discordUri, @$"ENERGY IS NEGATIEF - {x.New.State}");
                    Notify.NotifyPhoneVincent(@$"ENERGY IS NEGATIEF - {x.New.State}",
                        @"Je energy is negatief, dit kost geld.", false, 10);
                }
            });
    }
}