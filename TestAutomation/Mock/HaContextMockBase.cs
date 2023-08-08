using System.Reactive.Subjects;
using System.Reflection;
using System.Text.Json;
using NetDaemon.HassModel;
using NetDaemon.HassModel.Entities;

namespace TestAutomation.Mock;

public class HaContextMockBase : IHaContext, IHaContextMock
{
    private Dictionary<string, EntityState> EntityStates { get; } = new();
    private Subject<StateChange> StateAllChangeSubject { get; } = new();
    private Subject<Event> EventsSubject { get; } = new();

    public IObservable<StateChange> StateAllChanges() => StateAllChangeSubject;

    public EntityState? GetState(string entityId) =>
        EntityStates.TryGetValue(entityId, out var result) ? result : null;

    public IReadOnlyList<Entity> GetAllEntities() => EntityStates.Keys.Select(s => new Entity(this, s)).ToList();

    public virtual void CallService(string domain, string service, ServiceTarget? target = null, object? data = null)
    {
    }

    public Task<JsonElement?> CallServiceWithResponseAsync(string domain, string service, ServiceTarget? target = null, object? data = null)
    {
        throw new NotImplementedException();
    }

    public Area? GetAreaFromEntityId(string entityId) => null;

    public virtual void SendEvent(string eventType, object? data = null)
    {
    }

    public IObservable<Event> Events => EventsSubject;

    public void TriggerStateChange(Entity entity, string newStateValue, object? attributes = null)
    {
        var newState = new EntityState { State = newStateValue };
        if (attributes != null)
        {
            newState = newState.WithAttributes(attributes);
        }

        TriggerStateChange(entity.EntityId, newState);
    }

    public void TriggerStateChange(string entityId, EntityState newState)
    {
        var oldState = EntityStates.TryGetValue(entityId, out var current) ? current : null;
        EntityStates[entityId] = newState;
        StateAllChangeSubject.OnNext(new StateChange(new Entity(this, entityId), oldState, newState));
    }

    public void TriggerEvent(Event @event)
    {
        EventsSubject.OnNext(@event);
    }

    public virtual void VerifyServiceCalled(Entity entity, string domain, string service)
    {
    }
}

public interface IHaContextMock
{
    void TriggerStateChange(Entity entity, string newStateValue, object? attributes = null);
    void TriggerStateChange(string entityId, EntityState newState);
    void VerifyServiceCalled(Entity entity, string domain, string service);
}

public static class TestExtensions
{
    public static EntityState WithAttributes(this EntityState entityState, object attributes)
    {
        var copy = entityState with { };
        entityState.GetType().GetProperty("AttributesJson", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(
            copy, AsJsonElement(attributes));
        return copy;
    }

    private static JsonElement AsJsonElement(this object value)
    {
        var jsonString = JsonSerializer.Serialize(value);
        return JsonSerializer.Deserialize<JsonElement>(jsonString);
    }
}