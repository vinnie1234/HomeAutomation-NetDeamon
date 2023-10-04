using System.Reactive.Concurrency;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(GoogleAssistantButtonTranslate))]
[Focus]
public class GoogleAssistantButtonTranslate : BaseApp
{
    public GoogleAssistantButtonTranslate(IHaContext haContext, ILogger<GoogleAssistantButtonTranslate> logger, INotify notify, IScheduler scheduler)
        : base(haContext, logger, notify, scheduler)
    {
        foreach (var translateEntity in TranslationEntities())
            translateEntity.Key.WhenTurnsOn(_ =>
            {
                translateEntity.Key.TurnOff();
                translateEntity.Value.Press();
            });
    }

    private Dictionary<InputBooleanEntity, InputButtonEntity> TranslationEntities()
    {
        return new Dictionary<InputBooleanEntity, InputButtonEntity>
        {
            { Entities.InputBoolean.StartFriends, Entities.InputButton.StartFriends },
            { Entities.InputBoolean.Restartnetdaemon, Entities.InputButton.Restartnetdaemon },
            { Entities.InputBoolean.Pixelgivenextfeedeary, Entities.InputButton.Pixelgivenextfeedeary },
            { Entities.InputBoolean.Emptypetsnowy, Entities.InputButton.Emptypetsnowy },
            { Entities.InputBoolean.Cleanpetsnowy, Entities.InputButton.Cleanpetsnowy },
            { Entities.InputBoolean.Feedcat, Entities.InputButton.Feedcat }
        };
    }
}