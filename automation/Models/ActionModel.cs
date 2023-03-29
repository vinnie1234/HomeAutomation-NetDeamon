using System.Text.Json.Serialization;

namespace Automation.Models;

public record ActionModel
{
    [JsonPropertyName("action")]
    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    public string Action { get; set; } = default!;

    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    [JsonPropertyName("title")]
    public string Title { get; set; } = default!;

    // ReSharper disable once PropertyCanBeMadeInitOnly.Global
    // ReSharper disable once UnusedAutoPropertyAccessor.Global
    [JsonPropertyName("uri")]
    public string Uri { get; set; } = default!;

    public Action? Func;
}