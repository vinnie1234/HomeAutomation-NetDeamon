using System.Reactive.Concurrency;
using Automation.Enum;
using static Automation.Globals;

namespace Automation.apps.General;

//[Focus]
[NetDaemonApp(Id = nameof(AwayManager))]
public class AwayManager : BaseApp
{
    private bool _backHome;

    public AwayManager(
        IHaContext ha,
        ILogger<AwayManager> logger,
        INotify notify,
        IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        TriggersHandler();
        VincentHomeHandler();
        VincentAwayCheck();
    }
    
    private void VincentAwayCheck()
    {
        Entities.Person.VincentMaarschalkerweerd
            .StateChanges()
            .WhenStateIsFor(x => x?.State != "home" && Entities.InputBoolean.Away.IsOff(),
                TimeSpan.FromMinutes(2), Scheduler)
            .Subscribe(_ =>
            {
                Notify.NotifyPhoneVincent(@"Het lijkt er op dat je weg bent!",
                    @"Je gaat weg zonder wat te zeggen...",
                    true,
                    10,
                    new List<ActionModel>
                    {
                        new(action: @"SETAWAY", title: @"Ik ben weg",
                            func: () => { Entities.InputBoolean.Away.TurnOn(); })
                    });
            });
    }

    private void VincentHomeHandler()
    {
        Entities.Person.VincentMaarschalkerweerd
            .StateChanges()
            .Where(x => x.Old?.State != "home" && 
                        x.New?.State == "home" && 
                        Entities.InputBoolean.Away.IsOn())
            .Subscribe(_ => Entities.InputBoolean.Away.TurnOff());
    }

    private void TriggersHandler()
    {
        Entities.InputBoolean.Away.WhenTurnsOn(_ => AwayHandler());
        Entities.InputBoolean.Away.WhenTurnsOff(_ => { _backHome = true; });
        Entities.BinarySensor.GangMotion.WhenTurnsOn(_ => WelcomeHome());
    }

    private void AwayHandler()
    {
        _backHome = false;

        if (OfficeDays.Contains(DateTime.Now.DayOfWeek)
            && DateTime.Now.Hour < 9
            && Entities.InputBoolean.Holliday.IsOff())
            Notify.NotifyPhoneVincent(@"Werkse Vincent", @"Succes op kantoor :)", false, 5);
        else
            Notify.NotifyPhoneVincent(@"Tot ziens", @"Je laat je huis weer alleen :(", false, 5);

        Entities.Light.TurnAllOff();
        Entities.MediaPlayer.Tv.TurnOff();
        Entities.MediaPlayer.AvSoundbar.TurnOff();
    }

    private void WelcomeHome()
    {
        var houseState = GetHouseState(Entities);

        if (_backHome)
        {
            NotifyVincentHome(houseState);

            SetLightScene(houseState);

            _backHome = false;

            Scheduler.Schedule(TimeSpan.FromSeconds(15), () =>
            {
                var message = @"Welkom thuis Vincent!";
                if (Entities.Sensor.SnowyPetFeederStatus.State == "insufficient")
                    message += @" Het eten van Pixel is bijna op!";

                Notify.NotifyHouse("welcomeHome", message, true);
            });
        }
    }

    private void SetLightScene(HouseStateEnum houseState)
    {
        switch (houseState)
        {
            case HouseStateEnum.Morning:
                Entities.Scene.Woonkamermorning.TurnOn();
                break;
            case HouseStateEnum.Day:
                Entities.Scene.Woonkamerday.TurnOn();
                break;
            case HouseStateEnum.Evening:
                Entities.Scene.Woonkamerevening.TurnOn();
                break;
            case HouseStateEnum.Night:
                Entities.Scene.Woonkamernight.TurnOn();
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(houseState), $"{houseState} is not a valid house state!");
        }
    }

    private void NotifyVincentHome(HouseStateEnum houseState)
    {
        Notify.NotifyPhoneVincent(@"Welkom thuis Vincent",
            @$"De huis status is nu: {houseState}. Je lampen worden voor je ingesteld.",
            true,
            action: new List<ActionModel>
            {
                new(action: @"TURNONTV", title: @"TV Aanzetten", func: () => { Entities.MediaPlayer.Tv.TurnOn(); })
            });
    }

    // ReSharper disable once UnusedMember.Local
    private void HandleVacuumCleaner()
    {
        //todo, still need to build
        throw new NotImplementedException();
    }
}