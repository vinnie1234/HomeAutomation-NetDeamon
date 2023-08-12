using System.Reactive.Concurrency;
using System.Threading.Tasks;
using NetDaemon.Client;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(FunApp))]
//ReSharper disable once UnusedType.Global
public class FunApp : BaseApp
{
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public FunApp(IHaContext ha, ILogger<FunApp> logger, INotify notify, IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        Entities.Sensor.Ps5turnon.StateChanges().Where(x => x.New?.State == "on")
            .SubscribeAsync(x =>
            {
                Ps5TurnOn();
                return Task.CompletedTask;
            });
    }

    private void Ps5TurnOn()
    {
        if (DateTime.Now.DayOfWeek == DayOfWeek.Wednesday && DateTime.Now.Hour > 19)
            Notify.NotifyHouse(@"Dewin",
                @"Goede avond Dewin, ben je er klaar voor om weer vernederd te worden door Vincent?", true);
    }
}