using Automation;
using Automation.Helpers;

namespace TestAutomation;

public class GeneralTests
{
    [Fact]
    public void ShouldReadConfig()
    {
        var zedarDeviceId = ConfigManager.GetValueFromConfig("ZedarDeviceId");
        
        Assert.True(!string.IsNullOrEmpty(zedarDeviceId));
    }
}