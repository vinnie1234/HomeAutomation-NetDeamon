using System.Threading.Tasks;

namespace Automation.Interfaces;

public interface INotify
{
    Task NotifyHouse(string message);

    void NotifyGsmVincent(
        string title,
        string message,
        List<ActionModel>? action = null,
        string? image = null,
        string? channel = null,
        string? vibrationPattern = null,
        string? ledColor = null);

    // ReSharper disable once UnusedMember.Global
    void NotifyGsmVincentTts(string message);
}