using System.Text.Json.Serialization;

// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable AutoPropertyCanBeMadeGetOnly.Global

namespace Automation.Models;

// ReSharper disable once ClassNeverInstantiated.Global
public class EnergyPriceModel
{
    public EnergyPriceModel(DateTime startTime, double priceCtPerKwh, DateTime endTime)
    {
        StartTime = startTime;
        PriceCtPerKwh = priceCtPerKwh;
        EndTime = endTime;
    }

    [JsonPropertyName("start_time")]
    public DateTime StartTime { get; set; }
    
    [JsonPropertyName("end_time")]
    public DateTime EndTime { get; set; }
    
    [JsonPropertyName("price_ct_per_kwh")]
    public double PriceCtPerKwh { get; set; }
}