using System;
using System.IO;
using Newtonsoft.Json;

public class AppConfig
{
    public int DisplayType { get; set; } = 1;
    public int TemperatureUnit { get; set; } = 0;
    public int CpuTemperatureSensor { get; set; } = 0;
    public int CpuFanSensor { get; set; } = 0;
    public int PumpFanSensor { get; set; } = 0;
    public int MaxCpuFanRpm { get; set; } = 2000;
    public int MaxPumpFanRpm { get; set; } = 5200;
}

public static class ConfigManager
{
    private static string GetFullPath(string fileName)
    {
        string basePath = AppDomain.CurrentDomain.BaseDirectory;
        return Path.Combine(basePath, fileName);
    }

    public static AppConfig Load(string fileName)
    {
        string filePath = GetFullPath(fileName);

        if (!File.Exists(filePath))
        {
            return new AppConfig();
        }

        string json = File.ReadAllText(filePath);
        return JsonConvert.DeserializeObject<AppConfig>(json);
    }

    public static void Save(string fileName, AppConfig config)
    {
        string filePath = GetFullPath(fileName);
        string json = JsonConvert.SerializeObject(config, Formatting.Indented);
        File.WriteAllText(filePath, json);
    }
}
