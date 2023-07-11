using System.Text.Json.Serialization;

namespace Automation.Models;

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