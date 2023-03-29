// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable PropertyCanBeMadeInitOnly.Global

namespace Automation.Models;

public class LightStateModel
{
    public string? EntityId { get; set; }
    public IReadOnlyList<double>? RgbColors { get; set; }
    public double? Brightness { get; set; }
    public double? ColorTemp { get; set; }

    public bool IsOn { get; set; }

    public IReadOnlyList<string>? SupportedColorModes { get; set; }
}