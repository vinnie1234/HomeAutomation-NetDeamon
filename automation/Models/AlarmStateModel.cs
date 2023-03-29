using Newtonsoft.Json;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable once ClassNeverInstantiated.Global

namespace Automation.Models;

public class AlarmStateModel
{
    [JsonProperty("alarm_id")]
    public string? AlarmId { get; set; }

    [JsonProperty("fire_time")]
    public int? FireTime { get; set; }

    [JsonProperty("local_time")]
    public string? LocalTime { get; set; }

    [JsonProperty("local_time_iso")]
    public DateTime? LocalTimeIso { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("label")]
    public object? Label { get; set; }

    [JsonProperty("recurrence")]
    public List<int>? Recurrence { get; set; }

    [JsonProperty("EntityId")]
    public string? EntityId { get; set; }
}