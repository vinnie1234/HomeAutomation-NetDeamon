using System.Threading.Tasks;

namespace Automation.Interfaces;

public interface INotify
{
    Task NotifyHouse(string title, string message, bool canAlwaysSendNotification, double? sendAfterMinutes = null);

    void NotifyGsmVincent(
        string title,
        string message,
        bool canAlwaysSendNotification,
        double? sendAfterMinutes = null,
        List<ActionModel>? action = null,
        string? image = null,
        string? channel = null,
        string? vibrationPattern = null,
        string? ledColor = null);

    // ReSharper disable once UnusedMember.Global
    void NotifyGsmVincentTts(string title, string message, bool canAlwaysSendNotification, double? sendAfterMinutes = null);

    void ResetNotificationHistoryForNotificationTitle(string title);
}