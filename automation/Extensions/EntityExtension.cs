namespace Automation.Extensions;

/// <summary>
/// Provides Extension methods for Entities
/// </summary>
public static class EntityExtensions
{
    public static void WhenTurnsOn<T, TAttributes>(this Entity<T, EntityState<TAttributes>, TAttributes> entity,
        Action<StateChange<T, EntityState<TAttributes>>> observer, int throttleInSeconds = 0)
        where TAttributes : class
        where T : Entity<T, EntityState<TAttributes>, TAttributes>
    {
        entity.StateChanges().Throttle(TimeSpan.FromSeconds(throttleInSeconds))
            .Where(c => c.Old?.IsOff() == true && (c.New?.IsOn() ?? false))
            .Subscribe(observer);
    }

    public static void WhenTurnsOff<T, TAttributes>(this Entity<T, EntityState<TAttributes>, TAttributes> entity,
        Action<StateChange<T, EntityState<TAttributes>>> observer, int throttleInSeconds = 0)
        where TAttributes : class
        where T : Entity<T, EntityState<TAttributes>, TAttributes>
    {
        entity.StateChanges().Throttle(TimeSpan.FromSeconds(throttleInSeconds))
            .Where(c => c.Old?.IsOn() == true && (c.New?.IsOff() ?? false))
            .Subscribe(observer);
    }
}