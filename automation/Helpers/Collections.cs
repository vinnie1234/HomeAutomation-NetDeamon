namespace Automation.Helpers;

public static class Collections
{
    public static Dictionary<NumericSensorEntity, string> GetAllBattySensors(Entities entities)
    {
        return new Dictionary<NumericSensorEntity, string>
        {
            { entities.Sensor.BadkamerBattery, @"Wall switch Badkamer" },
            { entities.Sensor.BadkamerBattery2, @"Hue switch Badkamer" },
            { entities.Sensor.BadkamermotionBattery, @"Motion Detector Badkamer" },
            { entities.Sensor.SwitchBadkamerSpiegelBattery, @"Hue switch Badkamerspiegel" },
            { entities.Sensor.GangBattery, "Motion Detector Gang" },
            { entities.Sensor.GangBattery2, "Hue switch switch Gang" },
            { entities.Sensor.HalBattery, "Wall switch switch Gang" },
            { entities.Sensor.BergingBattery, @"Motion Detector Berging" },
            { entities.Sensor.WoonkamerBattery, @"Hue switch woonkamer" },
            { entities.Sensor.WoonkamerBattery2, @"Wall switch Woonkamer" },
            { entities.Sensor.SlaapkamerBattery, @"Wall switch Slaapkamer" },
            { entities.Sensor.SwitchBadkamerSpiegelBattery, @"Wall switch Badkamer" },
            { entities.Sensor.Rollerblind0001Battery, @"Rolluik Slaapkamer" },
            { entities.Sensor.BotA801Battery, @"Switchbot" },
            { entities.Sensor.KeukenAfstandbediening, @"Keuken afstandbediening" },
            //{entities.Sensor.JaapBatteryLevel, "Jaap"},
        };
    }

    public static Dictionary<InputDatetimeEntity, InputNumberEntity> GetFeedTimes(Entities entities)
    {
        return new Dictionary<InputDatetimeEntity, InputNumberEntity>
        {
            { entities.InputDatetime.Zedarfeedfirsttime, entities.InputNumber.Zedarfeedfirstamound },
            { entities.InputDatetime.Zedarfeedsecondtime, entities.InputNumber.Zedarfeedsecondamound },
            { entities.InputDatetime.Zedarfeedthirdtime, entities.InputNumber.Zedarfeedthirdamound },
            { entities.InputDatetime.Zedarfeedfourthtime, entities.InputNumber.Zedarfeedfourthamound }
        };
    }

    public static Dictionary<NumericSensorEntity, string> GetAllTemperatureSensors(Entities entities)
    {
        return new Dictionary<NumericSensorEntity, string>
        {
            { entities.Sensor.BadkamerTemperature, @"Badkamer" },
            { entities.Sensor.BergingTemperature, @"Berging" },
            { entities.Sensor.GangTemperature, @"Gang" },
        };
    }
}