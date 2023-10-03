using System.Reactive.Concurrency;

namespace Automation.apps.General;

[NetDaemonApp(Id = nameof(GoogleAssistantButtonTranslate))]
public class GoogleAssistantButtonTranslate : BaseApp
{
    protected GoogleAssistantButtonTranslate(IHaContext haContext, ILogger logger, INotify notify, IScheduler scheduler)
        : base(haContext, logger, notify, scheduler)
    {
        foreach (var translateEntity in TranslationEntities())
            translateEntity.Key.WhenTurnsOn(_ =>
            {
                translateEntity.Value.Press();
                translateEntity.Key.TurnOff();
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