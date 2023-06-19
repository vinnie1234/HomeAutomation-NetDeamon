namespace Automation.Models;

public class NotificationModel
{
    public NotificationModel(string name, string value, DateTime lastSendNotification)
    {
        Name = name;
        Value = value;
        LastSendNotification = lastSendNotification;
    }

    public string Name { get; }
    
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    public string? Value { get; set; }
    public DateTime LastSendNotification { get; }
}