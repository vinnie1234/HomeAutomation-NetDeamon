using System.Reactive.Concurrency;
using System.Threading;
using Automation.Helpers;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(NetDaemon))]
public class NetDaemon : BaseApp, IDisposable
{
    private readonly string _discordLogChannel = ConfigManager.GetValueFromConfigNested("Discord", "Logs") ?? "";
    
    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public NetDaemon(IHaContext ha, ILogger<NetDaemon> logger,
        INotify notify, IScheduler scheduler, IDataRepository storage)
        : base(ha, logger, notify, scheduler)
    {
        var lightColor = storage.Get<IReadOnlyList<double>>("NetDaemonRestart");
        
        if (lightColor != null && lightColor.ToString() != "")
        {
            //cause the codegen changed the input from Object to IReadOnlyCollection<int> but leaves the output to IReadOnlyList<double> I need to translate the value;
            IReadOnlyCollection<int> lightColorInInt = new[] { (int)lightColor[0], (int)lightColor[1], (int)lightColor[2] };
            Entities.Light.Koelkast.TurnOn(rgbColor: lightColorInInt);
            storage.Save("NetDaemonRestart", "");
        }
        
        
        if(!Entities.InputBoolean.Sleeping.IsOn())
            Notify.NotifyHouse("Het huis is opnieuw opgestart", "Het huis is opnieuw opgestart", true);
        Notify.NotifyDiscord("Het huis is opnieuw opgestart", new[] { _discordLogChannel });

        Entities.InputButton.Restartnetdaemon.StateChanges().Subscribe(_ =>
        {
            storage.Save("NetDaemonRestart", Entities.Light.Koelkast.Attributes?.RgbColor);
            Entities.Light.Koelkast.TurnOn(colorName: "red");
            Notify.NotifyHouse("Het huis wordt opnieuw opgestart", "Het huis wordt opnieuw opgestart", true);
            
            Thread.Sleep(TimeSpan.FromSeconds(5));
            Services.Hassio.AddonRestart("c6a2317c_netdaemon5");
        });
    }
    
#pragma warning disable CA1816
    public void Dispose()
#pragma warning restore CA1816
    {
        Notify.NotifyDiscord("NetDaemon stopped", new[] { _discordLogChannel });
    }
}