using System.Reactive.Concurrency;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(FunApp))]
//ReSharper disable once UnusedType.Global
public class FunApp : BaseApp
{
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public FunApp(IHaContext ha, ILogger<FunApp> logger, INotify notify, IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        Entities.Sensor.Ps5turnon.StateChanges().Where(x => x.New?.State?.ToLower() == "on")
            .Subscribe(_ =>
            {
                Ps5TurnedOn();
            });

        Friends();
    }

    private void Ps5TurnedOn()
    {
        if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday && DateTime.Now.Hour > 19)
            Notify.NotifyHouse(@"Dewin",
                @"Goede avond Dewin, ben je er klaar voor om weer vernederd te worden door Vincent?", true);
    }

    private void Friends()
    {
        Entities.InputBoolean.StartFriends.WhenTurnsOn(x =>
        {
            Notify.SendMusicToHome("http://192.168.50.189:8123/local/Friends.mp3");
            Entities.InputBoolean.StartFriends.TurnOff();
            Entities.Light.Hal.TurnOn();
        });
    }
}