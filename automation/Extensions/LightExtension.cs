namespace Automation.Extensions;

public static class LightExtension
{
    public static void TurnAllOff(this LightEntities lightEntities)
    {
        var properties = lightEntities.GetType().GetProperties();

        foreach (var property in properties)
        {
            var light = (LightEntity)property.GetValue(lightEntities, null)!;
            if (light.EntityId == "light.rt_ax88u_led")
            {
                continue;
            }

            light.TurnOff(transition: 5);
        }
    }
}