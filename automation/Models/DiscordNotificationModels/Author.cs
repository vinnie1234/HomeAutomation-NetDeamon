using System.Text.Json.Serialization;

namespace Automation.Models.DiscordNotificationModels;

public class Author
{
    public Author(string name, Uri url, Uri iconUrl)
    {
        Name = name;
        Url = url;
        IconUrl = iconUrl;
    }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("url")]
    public Uri Url { get; set; }

    [JsonPropertyName("icon_url")]
    public Uri IconUrl { get; set; }
}