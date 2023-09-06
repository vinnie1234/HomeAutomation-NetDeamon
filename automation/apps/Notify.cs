using System.Threading.Tasks;
using Automation.Enum;
using static System.Enum;

namespace Automation.apps;

public class Notify : INotify
{
    private readonly Entities _entities;
    private readonly IServices _services;
    private readonly IHaContext _ha;
    private readonly IDataRepository _storage;

    public Notify(IHaContext ha, IDataRepository storage)
    {
        _ha = ha;
        _storage = storage;
        _entities = new Entities(ha);
        _services = new Services(ha);
    }

    public async Task NotifyHouse(string title, string message, bool canAlwaysSendNotification,
        double? sendAfterMinutes = null)
    {
        var canSendNotification = CanSendNotification(_storage, canAlwaysSendNotification, title, sendAfterMinutes);
        if (!canSendNotification) return;

        SaveNotification(_storage, title, message);

        _entities.MediaPlayer.HubVincent.VolumeSet(0.4);
        _entities.MediaPlayer.Woonkamer.VolumeSet(0.4);

        var tasks = new List<Task>
        {
            Task.Run(() => _services.Tts.CloudSay(_entities.MediaPlayer.HubVincent.EntityId, message)),
            Task.Run(() => _services.Tts.CloudSay(_entities.MediaPlayer.Woonkamer.EntityId, message))
        };

        await Task.WhenAll(tasks);
    }

    public void NotifyPhoneVincent(
        string title,
        string message,
        bool canAlwaysSendNotification,
        double? sendAfterMinutes = null,
        List<ActionModel>? action = null,
        string? image = null,
        string? channel = null,
        string? vibrationPattern = null,
        string? ledColor = null)
    {
        var canSendNotification = CanSendNotification(_storage, canAlwaysSendNotification, title, sendAfterMinutes);
        if (!canSendNotification) return;

        SaveNotification(_storage, title, message);

        var data = ConstructData(action, image: image, channel: channel, vibrationPattern: vibrationPattern);
        _services.Notify.MobileAppSmS908b(new NotifyMobileAppSmS908bParameters
            { Title = title, Message = message, Data = data });
    }

    public void NotifyPhoneVincentTts(string title, string message, bool canAlwaysSendNotification,
        double? sendAfterMinutes = null)
    {
        var canSendNotification = CanSendNotification(_storage, canAlwaysSendNotification, title, sendAfterMinutes);
        if (!canSendNotification) return;

        SaveNotification(_storage, title, message);

        var data = ConstructData(null, true, phoneMessage: message);
        _services.Notify.MobileAppSmS908b(new NotifyMobileAppSmS908bParameters
            { Message = "TTS", Data = data });
    }

    public void ResetNotificationHistoryForNotificationTitle(string title)
    {
        var oldData = _storage.Get<List<NotificationModel>>("notificationHistory") ?? new List<NotificationModel>();
        var data = oldData.FirstOrDefault(x => x.Name == title);
        if (data != null) oldData.Remove(data);

        _storage.Save("notificationHistory", oldData);
    }

    private void SubscribeToNotificationAction(Action func, string key, string title)
    {
        _ha.Events.Where(x => x.EventType == "mobile_app_notification_action")
            .Subscribe(x =>
            {
                var eventActionModel = x.DataElement?.ToObject<EventActionModel>();
                if (eventActionModel?.Action == key) func.Invoke();
            });
    }

    private RecordNotifyData ConstructData(List<ActionModel>? actions = null,
        bool tts = false,
        NotifyPriorityEnum priority = NotifyPriorityEnum.High,
        string? phoneMessage = null,
        string? image = null,
        string? channel = null,
        string? vibrationPattern = null)
    {
        //construct the data here
        RecordNotifyData data = new(
            priority: GetName(priority)?.ToLower(),
            ttl: 0,
            tag: null,
            color: "",
            sticky: "true");

        if (tts)
        {
            data.Channel = "process";
            data.TtsText = phoneMessage;
        }

        data.Channel = channel;
        data.VibrationPattern = vibrationPattern; //"100, 1000, 100, 1000, 100"
        data.LedColor = vibrationPattern;         //"red"
        data.Image = image;

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

    private static bool CanSendNotification(IDataRepository storage, bool canAlwaysSend, string title,
        double? sendAfterMinutes)
    {
        if (canAlwaysSend) return true;

        var notification = GetLastNotification(storage, title);

        sendAfterMinutes ??= 60;
        return DateTime.Now.AddMinutes((double)sendAfterMinutes) >= notification?.LastSendNotification;
    }

    private static void SaveNotification(IDataRepository storage, string title, string message)
    {
        var oldData = storage.Get<List<NotificationModel>>("notificationHistory") ?? new List<NotificationModel>();
        var data = oldData.FirstOrDefault(x => x.Name == title);
        if (data != null)
            data.Value = message;
        else
            oldData.Add(new NotificationModel(name: title, value: message, lastSendNotification: DateTime.Now));

        storage.Save("notificationHistory", oldData);
    }

    private static NotificationModel? GetLastNotification(IDataRepository storage, string title)
    {
        var oldData = storage.Get<List<NotificationModel>>("notificationHistory") ?? new List<NotificationModel>();
        return oldData.FirstOrDefault(x => x.Name == title);
    }
}