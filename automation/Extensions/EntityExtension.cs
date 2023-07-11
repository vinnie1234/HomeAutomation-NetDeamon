namespace Automation.Extensions;

/// <summary>
/// Provides Extension methods for Entities
/// </summary>
public static class EntityExtensions
{
    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IDisposable WhenTurnsOn<T, TAttributes>(
        this Entity<T, EntityState<TAttributes>, TAttributes> entity,
        Action<StateChange<T, EntityState<TAttributes>>> observer)
        where TAttributes : class
        where T : Entity<T, EntityState<TAttributes>, TAttributes>
        => entity.StateChanges().Where(c => c.Old?.IsOff() == true && (c.New?.IsOn() ?? false))
            .Subscribe(observer);

    // ReSharper disable once UnusedMethodReturnValue.Global
    public static IDisposable WhenTurnsOff<T, TAttributes>(
        this Entity<T, EntityState<TAttributes>, TAttributes> entity,
        Action<StateChange<T, EntityState<TAttributes>>> observer)
        where TAttributes : class
        where T : Entity<T, EntityState<TAttributes>, TAttributes>
        => entity.StateChanges().Where(c => c.Old?.IsOn() == true && (c.New?.IsOff() ?? false))
            .Subscribe(observer);
}