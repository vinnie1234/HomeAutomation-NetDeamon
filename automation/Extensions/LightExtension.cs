namespace Automation.Extensions;

public static class LightExtension
{
    public static void TurnAllOff(this LightEntities lightEntities)
    {
        var properties = lightEntities.GetType().GetProperties();

        foreach (var property in properties)
        {
            var light = (LightEntity)property.GetValue(lightEntities, null)!;
            // ReSharper disable once CommentTypo
            //todo light.tradfri_driver is temp
            if (light.EntityId is "light.rt_ax88u_led" or "light.tradfri_driver") continue;

            light.TurnOff(transition: 5);
        }
    }
}