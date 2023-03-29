namespace Automation.Extensions;

public static class JsonElementExtension
{
    public static T? ToObject<T>(this JsonElement element)
    {
        var json = element.GetRawText();
        return JsonSerializer.Deserialize<T>(json);
    }
}