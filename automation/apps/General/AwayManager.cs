using System.Collections;
using System.Threading;
using Automation.Enum;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(AwayManager))]
// ReSharper disable once UnusedType.Global
public class AwayManager : BaseApp
{
    private bool _backHome;

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public AwayManager(IHaContext ha, ILogger<AwayManager> logger, INotify notify)
        : base(ha, logger, notify)
    {
        Entities.InputBoolean.Away.WhenTurnsOn(_ => AwayHandler());
        Entities.InputBoolean.Away.WhenTurnsOff(_ => { _backHome = true; });
        Entities.BinarySensor.GangMotion.WhenTurnsOn(_ => WelcomeHome());

        Entities.Person.VincentMaarschalkerweerd
            .StateChanges()
            .Where(x => x.Old?.State != "home" && x.New?.State == "home" && Entities.InputBoolean.Away.IsOn())
            .Subscribe(_ => Entities.InputBoolean.Away.TurnOff());
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
                    Entities.Scene.WoonkamerMorning.TurnOn();
                    break;
                case HouseStateEnum.Day:
                    Entities.Scene.WoonkamerDay.TurnOn();
                    break;
                case HouseStateEnum.Evening:
                    Entities.Scene.WoonkamerEvening.TurnOn();
                    break;
                case HouseStateEnum.Night:
                    Entities.Scene.WoonkamerNight.TurnOn();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
            
            Thread.Sleep(15000);
            var message = @"Welkom thuis Vincent!";
            if (Entities.BinarySensor.ZedarFeedContainer.IsOff())
                message += @" Het eten van de Pixel is bijna op!";
            
            Notify.NotifyHouse(message);
        }

        _backHome = false;
    }

    // ReSharper disable once UnusedMember.Local
    private void HandleVacuumCleaner()
    {
        throw new NotImplementedException();
    }
}