using Automation.Interfaces;
using HomeAssistantGenerated;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Reactive.Testing;
using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;
using Moq;

namespace TestAutomation.Helpers;

public class TestBase
{
    public TestContext Context = new();
    public Entities Entities => Context.GetRequiredService<Entities>();
    public HaContextMock HaMock => Context.GetRequiredService<HaContextMock>();
    public TestScheduler Scheduler => Context.GetRequiredService<TestScheduler>();
    public Mock<INotify> NotifyMock => Context.GetRequiredService<Mock<INotify>>();
    public Mock<ISettingsProvider> SettingsProviderMock => Context.GetRequiredService<Mock<ISettingsProvider>>();

    internal virtual void VerifyAllMocks()
    {
        HaMock.VerifyAll();
        NotifyMock.VerifyAll();
        SettingsProviderMock.VerifyAll();
    }

    internal virtual void ResetAllMocks()
    {
        HaMock.Reset();
        NotifyMock.Reset();
        SettingsProviderMock.Reset();
    }
}