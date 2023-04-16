namespace Automation.apps.Rooms.BedRoom;

[NetDaemonApp(Id = nameof(BedRoomLights))]
// ReSharper disable once UnusedType.Global
public class BedRoomLights : BaseApp
{
    // ReSharper disable once UnusedMember.Local
    private bool DisableLightAutomations => Entities.InputBoolean.Disablelightautomationbedroom.IsOn();
    
    public BedRoomLights(IHaContext ha, ILogger<BedRoomLights> logger, INotify notify)
        : base(ha, logger, notify)
    {
    }
}