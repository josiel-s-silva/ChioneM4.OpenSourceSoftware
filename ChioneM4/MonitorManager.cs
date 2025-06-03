using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace WaterCoolerM4
{
    public class SensorInfo
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public ISensor Sensor { get; set; }

        public override string ToString()
        {
            return $"{Name} (ID: {Id})";
        }
    }

    public class MonitorManager
    {
        private readonly Computer computer;
        private readonly List<SensorInfo> fanSensors = new List<SensorInfo>();
        private readonly List<SensorInfo> temperatureSensors = new List<SensorInfo>();

        public MonitorManager()
        {
            computer = new Computer
            {
                IsCpuEnabled = true,
                IsMotherboardEnabled = true
            };
            computer.Open();

            LoadTemperatureSensors();
            LoadFanSensors();
        }

        private void LoadFanSensors()
        {
            fanSensors.Clear();
            int idCounter = 0;

            foreach (IHardware hardware in computer.Hardware)
            {
                CollectFanSensorsRecursive(hardware, ref idCounter);
            }
        }

        private void CollectFanSensorsRecursive(IHardware hardware, ref int idCounter)
        {
            hardware.Update();

            foreach (ISensor sensor in hardware.Sensors)
            {
                if (sensor.SensorType == SensorType.Fan & sensor.Value > 0)
                {
                    fanSensors.Add(new SensorInfo
                    {
                        Id = idCounter++,
                        Name = $"{sensor.Name}",
                        Sensor = sensor
                    });
                }
            }

            foreach (IHardware subHardware in hardware.SubHardware)
            {
                CollectFanSensorsRecursive(subHardware, ref idCounter);
            }
        }

        private void LoadTemperatureSensors()
        {
            int idCounter = 0;

            foreach (IHardware hardware in computer.Hardware)
            {
                if (hardware.HardwareType == HardwareType.Cpu)
                {
                    hardware.Update();
                    foreach (ISensor sensor in hardware.Sensors)
                    {
                        if (sensor.SensorType == SensorType.Temperature)
                        {
                            temperatureSensors.Add(new SensorInfo
                            {
                                Id= idCounter++,
                                Name = $"{sensor.Name}",
                                Sensor = sensor
                            });
                        }
                    }
                }
            }
        }

        public List<SensorInfo> GetFanSensorList()
        {
            return fanSensors;
        }

        public List<SensorInfo> GetTemperatureSensorList()
        {
            return temperatureSensors;
        }
    }
}
