using System.Text.Json.Serialization;

namespace Automation.Models.DiscordNotificationModels;

public class DiscordNotificationModel
{
    [JsonPropertyName("embed")]
    public Embed? Embed { get; set; }
    
    [JsonPropertyName("images")]
    public string[]? Images { get; set; }
    
    [JsonPropertyName("urls")]
    public string[]? Urls { get; set; }

    [JsonPropertyName("verify_ssl")]
    public bool VerifySsl { get; set; }
}