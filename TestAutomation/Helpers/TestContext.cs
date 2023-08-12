using System.Reactive.Concurrency;
using Automation.apps;
using Automation.apps.General;
using Automation.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Moq;
using NetDaemon.HassModel;

namespace TestAutomation.Helpers;

public class TestContext : IServiceProvider
{
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();
    private readonly IServiceProvider _serviceProvider;

    public TestContext()
    {
        _serviceCollection.AddGeneratedCode();
        _serviceCollection.AddSingleton(_ => new HaContextMock(MockBehavior.Strict));
        _serviceCollection.AddTransient<IHaContext>(s => s.GetRequiredService<HaContextMock>().Object);
        _serviceCollection.AddSingleton<TestScheduler>();
        _serviceCollection.AddTransient<IScheduler>(s => s.GetRequiredService<TestScheduler>());

        _serviceCollection.AddSingleton(_ => new Mock<ILogger<Notify>>());
        _serviceCollection.AddTransient(s => s.GetRequiredService<Mock<ILogger<Notify>>>().Object);
        
        _serviceCollection.AddSingleton(_ => new Mock<ILogger<FunApp>>());
        _serviceCollection.AddTransient(s => s.GetRequiredService<Mock<ILogger<FunApp>>>().Object);
        
        _serviceCollection.AddSingleton(_ => new Mock<ISettingsProvider>(MockBehavior.Strict));
        _serviceCollection.AddTransient(s => s.GetRequiredService<Mock<ISettingsProvider>>().Object);

        _serviceCollection.AddSingleton(_ => new Mock<INotify>(MockBehavior.Strict));
        _serviceCollection.AddTransient(s => s.GetRequiredService<Mock<INotify>>().Object);
        
        _serviceCollection.AddSingleton(_ => new Mock<IDataRepository>(MockBehavior.Strict));
        _serviceCollection.AddTransient(s => s.GetRequiredService<Mock<IDataRepository>>().Object);
        
        _serviceProvider = _serviceCollection.BuildServiceProvider();
    }

    public object? GetService(Type serviceType) => _serviceProvider.GetService(serviceType);

    public T GetApp<T>() => ActivatorUtilities.GetServiceOrCreateInstance<T>(_serviceProvider);
}