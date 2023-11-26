using System.Reactive.Concurrency;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(FunApp))]
//[Focus]
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
        NewYear();
    }

    private void Ps5TurnedOn()
    {
        if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday && DateTime.Now.Hour >= 19)
            Notify.NotifyHouse(@"Dewin",
                @"Goede avond Dewin, ben je er klaar voor om weer vernederd te worden door Vincent?", true);
    }

    private void Friends()
    {
        Entities.InputButton.StartFriends.StateChanges()
            .Subscribe(_ =>
            {
                Notify.SendMusicToHome("http://192.168.50.189:8123/local/Friends.mp3");
                Entities.Light.Hal.TurnOn();
            });
    }

    private void NewYear()
    {
        Scheduler.ScheduleCron("10 58 23 31 12 *", () =>
        {
            Notify.SendMusicToHome("http://192.168.50.189:8123/local/HappyNewYear.mp3", 0.9);
        }, true);
        
        Scheduler.ScheduleCron("59 58 23 31 12 *", () =>
        {
            Entities.MediaPlayer.HeleHuis.VolumeSet(0.9);
        }, true);
    }
}