using NetDaemon.AppModel;

namespace TestAutomation.Helpers;

public class FakeAppConfig<T> : IAppConfig<T> where T : class, new()
{
    private readonly T _value;

    public FakeAppConfig(T instance)
    {
        _value = instance;
    }

    public T Value => _value;
}

// (IHaContext haContext, string entityId)