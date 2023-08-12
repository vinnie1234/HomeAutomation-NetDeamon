using Automation.apps;
using Automation.apps.General;
using Automation.Enum;
using Moq;
using TestAutomation.Helpers;
using Xunit;

namespace TestAutomation;

public class NotifyTest : TestBase
{
    [Fact]
    public void NotifyApp_Constructor_CheckEvents()
    {
        // Arrange
        ResetAllMocks();
        // Act
        var app = Context.GetApp<Notify>();

        // Assert
        VerifyAllMocks();
    }
}