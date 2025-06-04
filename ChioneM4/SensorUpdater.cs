using System;
using System.Threading;
using System.Threading.Tasks;
using System.Diagnostics;

namespace WaterCoolerM4
{
    public class SensorUpdater
    {
        private readonly MonitorManager monitorManager;
        private readonly AppConfig config;
        private readonly Action<float, float, float> onUpdate;
        private CancellationTokenSource cts;
        private Task updateTask;
        private SensorInfo cpuTempSensor;
        private SensorInfo cpuFanSensor;
        private SensorInfo pumpFanSensor;
        private bool disposed = false;

        public SensorUpdater(MonitorManager monitorManager, AppConfig config, Action<float, float, float> onUpdate)
        {
            this.monitorManager = monitorManager;
            this.config = config;
            this.onUpdate = onUpdate;
            SetSensorsFromConfig();
        }

        public void SetSensorsFromConfig()
        {
            var tempSensors = monitorManager.GetTemperatureSensorList();
            var fanSensors = monitorManager.GetFanSensorList();
            cpuTempSensor = (config.CpuTemperatureSensor >= 0 && config.CpuTemperatureSensor < tempSensors.Count) ? tempSensors[config.CpuTemperatureSensor] : null;
            cpuFanSensor = (config.CpuFanSensor >= 0 && config.CpuFanSensor < fanSensors.Count) ? fanSensors[config.CpuFanSensor] : null;
            pumpFanSensor = (config.PumpFanSensor >= 0 && config.PumpFanSensor < fanSensors.Count) ? fanSensors[config.PumpFanSensor] : null;
        }

        public void Start()
        {
            Stop();
            cts = new CancellationTokenSource();
            updateTask = Task.Run(() => UpdateLoop(cts.Token), cts.Token);
        }

        public void Stop()
        {
            if (cts != null)
            {
                cts.Cancel();
                updateTask?.Wait();

                cts.Dispose();
                cts = null;
            }
        }

        private async Task UpdateLoop(CancellationToken token)
        {
            while (!token.IsCancellationRequested)
            {
                try
                {
                    cpuTempSensor?.Sensor.Hardware.Update();
                    cpuFanSensor?.Sensor.Hardware.Update();
                    pumpFanSensor?.Sensor.Hardware.Update();

                    float cpuTemp = cpuTempSensor?.Sensor.Value ?? 0;
                    float cpuFan = cpuFanSensor?.Sensor.Value ?? 0;
                    float pumpFan = pumpFanSensor?.Sensor.Value ?? 0;

                    onUpdate?.Invoke(cpuTemp, cpuFan, pumpFan);
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"[SensorUpdater] Erro ao atualizar sensores: {ex.Message}");
                }
                await Task.Delay(1000, token);
            }
        }
    }
}
