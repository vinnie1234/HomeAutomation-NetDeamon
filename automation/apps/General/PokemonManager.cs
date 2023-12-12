using System.Reactive.Concurrency;
using Automation.Helpers;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(PokemonManager))]
public class PokemonManager : BaseApp
{
    private readonly string _discordDewinChannel = ConfigManager.GetValueFromConfigNested("Discord", @"Dewin") ?? "";

    // ReSharper disable once SuggestBaseTypeForParameterInConstructor
    public PokemonManager(IHaContext ha, ILogger<PokemonManager> logger,
        INotify notify, IScheduler scheduler)
        : base(ha, logger, notify, scheduler)
    {
        Entities.Person
            .VincentMaarschalkerweerd
            .StateChanges()
            .Where(x => x.Old?.State?.ToLower() == "pokemon raid hour" && DateTimeOffset.Now.DayOfWeek == DayOfWeek.Wednesday && DateTimeOffset.Now.Hour >= 18)
            .Subscribe(_ =>
            {
                notify.NotifyDiscord(@"Vincent rijd nu weg van Pokémon", new[] { _discordDewinChannel });
            });
    }
}