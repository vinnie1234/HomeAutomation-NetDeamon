using System.Collections;
using Automation.Enum;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(AwayManager))]
public class AwayManager : BaseApp
{
    private bool _backHome;
    
    public AwayManager(IHaContext ha, ILogger<AwayManager> logger, INotify notify, INetDaemonScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        Entities.InputBoolean.Away.WhenTurnsOn(_ => AwayHandler());
        Entities.InputBoolean.Away.WhenTurnsOff(_ => { _backHome = true; });
        Entities.BinarySensor.GangMotion.WhenTurnsOn(_ => WelcomeHome());

        Entities.Person.VincentMaarschalkerweerd
            .StateChanges()
            .Where(x => x.Old?.State != "home" && x.New?.State == "home" && Entities.InputBoolean.Away.IsOn())
            .Subscribe(_ => Entities.InputBoolean.Away.TurnOff());
        
        Entities.Person.VincentMaarschalkerweerd
            .StateChanges()
            .WhenStateIsFor(x => x?.State != "home" && Entities.InputBoolean.Away.IsOff(),
                TimeSpan.FromMinutes(2))
            .Subscribe(_ =>
            {
                Notify.NotifyGsmVincent(@"Het lijkt er op dat je weg bent!",
                    @"Je gaat weg zonder wat te zeggen...",
                    new List<ActionModel>
                    {
                        new()
                        {
                            Action = @"SETAWAY",
                            Title = @"Ik ben weg",
                            Func = () =>
                            {
                                Entities.InputBoolean.Away.TurnOn();
                            }
                        }
                    });
            });
    }

    private void AwayHandler()
    {
        _backHome = false;
        if (((IList)Globals.OfficeDays).Contains(DateTime.Now.DayOfWeek) && DateTime.Now.Hour < 9 &&
            Entities.InputBoolean.Holliday.IsOff())
        {
            Notify.NotifyGsmVincent(@"Werkse Vincent", @"Succes op kantoor :)");
        }
        else
        {
            Notify.NotifyGsmVincent(@"Tot ziens", @"Je laat je huis weer alleen :(");
        }

        Entities.Light.TurnAllOff();
        Entities.MediaPlayer.Tv.TurnOff();
        Entities.MediaPlayer.AvSoundbar.TurnOff();
    }

    private void WelcomeHome()
    {
        var houseState = Globals.GetHouseState(Entities);

        if (_backHome)
        {
            Notify.NotifyGsmVincent(@"Welkom thuis Vincent",
                @$"De huis status is nu: {houseState}. Je lampen worden voor je ingesteld.",
                new List<ActionModel>
                {
                    new()
                    {
                        Action = @"TURNONTV",
                        Title = @"TV Aanzetten",
                        Func = () =>
                        {
                            Entities.MediaPlayer.Tv.TurnOn();
                            Notify.ClearPhone();
                        }
                    }
                });

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
                    throw new ArgumentOutOfRangeException();
            }

            _backHome = false;
            
            Scheduler.RunIn(TimeSpan.FromSeconds(15), () =>
            {
                var message = @"Welkom thuis Vincent!";
                if (Entities.BinarySensor.ZedarFeedContainer.IsOff())
                    message += @" Het eten van de Pixel is bijna op!";

                Notify.NotifyHouse(message);
            });
        }
    }

    // ReSharper disable once UnusedMember.Local
    private void HandleVacuumCleaner()
    {
        throw new NotImplementedException();
    }
}