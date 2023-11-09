using System.Text.Json.Serialization;

namespace Automation.Models.Yts;

public class TitleDetail
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;
    
    [JsonPropertyName("language")]
    public object Language { get; set; } = default!;
    
    [JsonPropertyName("base")]
    public string Base { get; set; } = default!;
    
    [JsonPropertyName("value")]
    public string Value { get; set; } = default!;
}