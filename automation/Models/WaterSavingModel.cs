// ReSharper disable UnusedAutoPropertyAccessor.Global

namespace Automation.Models;

public class WaterSavingModel
{
    public Guid? Id { get; init; }
    public string? Value { get; set; }
    public string? Guess { get; set; }
    
    public DateTimeOffset DateTime { get; set; }
}