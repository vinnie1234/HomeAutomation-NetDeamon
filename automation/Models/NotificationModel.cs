namespace Automation.Models;

public class NotificationModel
{
    public string Name { get; init; } = default!;
    
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string Value { get; set; } = default!;
    public DateTime LastSendNotification { get; init; }
}