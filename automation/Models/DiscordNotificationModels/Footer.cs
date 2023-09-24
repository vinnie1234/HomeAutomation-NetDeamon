using System.Text.Json.Serialization;

namespace Automation.Models.DiscordNotificationModels;

public class Footer
{
    public Footer(string text, Uri iconUrl)
    {
        Text = text;
        IconUrl = iconUrl;
    }

    [JsonPropertyName("text")]
    public string Text { get; set; }

    [JsonPropertyName("icon_url")]
    public Uri IconUrl { get; set; }
}