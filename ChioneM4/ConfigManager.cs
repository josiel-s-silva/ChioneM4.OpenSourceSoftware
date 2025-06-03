using System.IO;
using Newtonsoft.Json;
public class AppConfig
{
    public int DisplayType { get; set; }
    public int CpuTemperatureSensor { get; set; }
    public int CpuFanSensor { get; set; }
    public int PumpFanSensor { get; set; }
    public int MaxCpuFanRpm { get; set; }
    public int MaxPumpFanRpm { get; set; }
}

public static class ConfigManager
{
    public static AppConfig Load(string filePath)
    {
        if (!File.Exists(filePath))
        {
            return new AppConfig
            {
                DisplayType = 1,
                CpuTemperatureSensor = 0,
                CpuFanSensor = 0,
                PumpFanSensor = 0,
                MaxCpuFanRpm = 2000,
                MaxPumpFanRpm = 5200
            };
        }

        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<AppConfig>(json);
    }

    public static void Save(string filePath, AppConfig config)
    {
        string json = JsonConvert.SerializeObject(config, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
}