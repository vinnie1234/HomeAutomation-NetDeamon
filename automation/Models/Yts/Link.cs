using System.Text.Json.Serialization;

namespace Automation.Models.Yts;

public class Link
{
    [JsonPropertyName("rel")]
    public string Rel { get; set; } = default!;
    
    [JsonPropertyName("type")]
    public string Type { get; set; } = default!;
    
    [JsonPropertyName("href")]
    public string Href { get; set; } = default!;
    
    [JsonPropertyName("length")]
    public string Length { get; set; } = default!;
}