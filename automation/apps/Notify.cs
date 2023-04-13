using System.Threading.Tasks;
using Automation.Enum;

namespace Automation.apps;

public class Notify : INotify
{
    private readonly Entities _entities;
    private readonly IServices _services;
    private readonly IHaContext _ha;

    public Notify(IHaContext ha)
    {
        _ha = ha;
        _entities = new Entities(ha);
        _services = new Services(ha);
    }

    public async Task NotifyHouse(string message)
    {
        _entities.MediaPlayer.HubVincent.VolumeSet(0.4);
        _entities.MediaPlayer.Woonkamer.VolumeSet(0.4);

        var tasks = new List<Task>
        {
            Task.Run(() => _services.Tts.GoogleSay(new TtsGoogleSayParameters
                { EntityId = _entities.MediaPlayer.HubVincent.EntityId, Message = message, Language = "nl" })),
            Task.Run(() => _services.Tts.GoogleSay(new TtsGoogleSayParameters
                { EntityId = _entities.MediaPlayer.Woonkamer.EntityId, Message = message, Language = "nl" }))
        };

        await Task.WhenAll(tasks);
    }

    public void NotifyGsmVincent(
        string title,
        string message,
        List<ActionModel>? action = null,
        string? image = null,
        string? channel = null,
        string? vibrationPattern = null,
        string? ledColor = null)
    {
        var data = ConstructData(action, image: image, channel: channel, vibrationPattern: vibrationPattern,
            ledColor: ledColor);
        _services.Notify.MobileAppSmS908b(new NotifyMobileAppSmS908bParameters
            { Title = title, Message = message, Data = data });
    }

    public void NotifyGsmVincentTts(string message)
    {
        var data = ConstructData(null, true, phoneMessage: message);
        _services.Notify.MobileAppSmS908b(new NotifyMobileAppSmS908bParameters
            { Message = "TTS", Data = data });
    }

    public void ClearPhone()
    {
        var data = ConstructData();
        _services.Notify.MobileAppSmS908b(new NotifyMobileAppSmS908bParameters
            { Title = "", Message = "clear_notification", Data = data });
    }

    private void SubscribeToNotificationAction(Action func, string key, string title)
    {
        _ha.Events.Where(x => x.EventType == "mobile_app_notification_action")
            .Subscribe(x =>
            {
                var eventActionModel = x.DataElement?.ToObject<EventActionModel>();
                if (eventActionModel?.Action1Key == key && eventActionModel.Action1Title == title)
                {
                    func.Invoke();
                }
            });
    }

    private RecordNotifyData ConstructData(List<ActionModel>? actions = null,
        bool tts = false,
        NotifyPriorityEnum priority = NotifyPriorityEnum.High,
        string? phoneMessage = null,
        string? image = null,
        string? channel = null,
        string? vibrationPattern = null,
        string? ledColor = null)
    {
        //construct the data here
        RecordNotifyData data = new()
        {
            Priority = System.Enum.GetName(priority)?.ToLower(),
            Ttl = 0,
            Tag = null,
            Color = "",
            Sticky = "true"
        };

        if (tts)
        {
            data.Channel = "process";
            data.TtsText = phoneMessage;
        }

        if (!string.IsNullOrEmpty(channel))
        {
            data.Channel = channel;
        }

        if (!string.IsNullOrEmpty(vibrationPattern))
        {
            data.VibrationPattern = vibrationPattern; //"100, 1000, 100, 1000, 100"
        }

        if (!string.IsNullOrEmpty(ledColor))
        {
            data.LedColor = vibrationPattern; //"red"
        }

        if (!string.IsNullOrEmpty(image)) data.Image = image;

        if (actions != null)
        {
            if (actions.Count > 3) throw new Exception("To many actions");
            foreach (var action in actions.Where(action => action.Func != null))
            {
                SubscribeToNotificationAction(action.Func!, action.Action, action.Title);
                action.Func = null;
            }

            data.Actions = actions;
        }

        return data;
    }
}