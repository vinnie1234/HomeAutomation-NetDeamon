namespace Automation.Models;

/// <summary>
/// Represents a person and their states.
/// </summary>
public class PersonModel
{
    public bool IsSleeping { get; }
    public bool IsDriving { get; }
    public bool IsHome { get; }
    public string? TravelDirection { get; }
    public string? Location { get; }

    public PersonModel(bool sleeping, bool driving, bool home, string? travelDirection, string? location)
    {
        IsSleeping = sleeping;
        IsDriving = driving;
        IsHome = home;
        TravelDirection = travelDirection;
        Location = location;
    }
}