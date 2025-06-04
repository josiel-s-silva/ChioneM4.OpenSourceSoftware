using CHIONE_M4.Usb;
using LibreHardwareMonitor.Hardware;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace WaterCoolerM4
{
    /// <summary>
    /// Interação lógica para MainWindow.xam
    /// </summary>
    public partial class MainWindow : Window
    {
        private SensorUpdater sensorUpdater;
        private const string DefaultConfigPath = "config.json";
        private const string DefaultFindPid = "B533";

        private AppConfig Config;

        private ClassUSBListener classUSBListener;
        private ClassUSBOnline classUSBOnline;
        private ClassUSBPort classUSBPort = new ClassUSBPort();

        private MonitorManager monitorManager = new MonitorManager();

        private byte stnLightOnOf_Fan = 1;
        private byte stnLightOnOf_Pum = 1;
        private byte stnLightOnOf_Whi = 1;
        private byte stnLightOnOf_Blu = 1;
        private byte stnLightOnOf_Lig = 1;
        public MainWindow()
        {
            InitializeComponent();

            // Load configuration
            Config = ConfigManager.Load(DefaultConfigPath);

            // Start the USB listener
            classUSBListener = new ClassUSBListener(this, listenHook);
            classUSBOnline = new ClassUSBOnline();
            UsbOnline();

            // Config water cooler display
            ConfigScreen();

            // Set the initial values for the text boxes
            loadUI();

            // Start the sensor updater
            sensorUpdater = new SensorUpdater(monitorManager, Config, OnSensorUpdate);
            sensorUpdater.Start();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Clear display
            ClearCpuTemp();
            UpdateFanDisplay(0);
            UpdatePumpDisplay(0);
        }

        private void OnSensorUpdate(float cpuTemp, float cpuFan, float pumpFan)
        {
            try
            {
                int cpuFanPorcentage = (int)(cpuFan / (Config.MaxCpuFanRpm > 0 ? Config.MaxCpuFanRpm : 1) * 100);
                int pumpFanPorcentage = (int)(pumpFan / (Config.MaxPumpFanRpm > 0 ? Config.MaxPumpFanRpm : 1) * 100);

                Debug.WriteLine($"CPU Temp: {cpuTemp} °C, CPU Fan: {cpuFan} RPM, Pump Fan: {pumpFan} RPM");
                Debug.WriteLine($"CPU Fan Porcentage: {cpuFanPorcentage} %, Pump Fan Porcentage: {pumpFanPorcentage} %");

                SetCpuTemp(cpuTemp);
                UpdateFanDisplay(cpuFanPorcentage);
                UpdatePumpDisplay(pumpFanPorcentage);

                Application.Current.Dispatcher.Invoke(() =>
                {
                    if (Config.TemperatureUnit == 0)
                    {
                        cpuTemperatureLabel.Content = $"{cpuTemp:00.0} °C";
                    }
                    else
                    {
                        cpuTemperatureLabel.Content = Utils.celsiusToFahrenheit(cpuTemp) + "°F";
                    }
                    cpuFanLabel.Content = $"{cpuFan:00.0} RPM";
                    pumpFanLabel.Content = $"{pumpFan:00.0} RPM";
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Erro ao atualizar UI: {ex.Message}");
            }
        }

        private void loadUI()
        {
            // Populate the combo boxs with CPU temperature sensors
            foreach (SensorInfo sensor in monitorManager.GetTemperatureSensorList())
            {
                Debug.WriteLine($"Sensor ID: {sensor.Id}, Name: {sensor.Name}");
                cpuTemperatureSensor.Items.Add(sensor);
            }

            Debug.WriteLine("Total CPU Temp Sensors: " + monitorManager.GetTemperatureSensorList().Count);

            // Populate the combo boxs with fan sensors
            foreach (SensorInfo sensor in monitorManager.GetFanSensorList())
            {
                Debug.WriteLine($"Sensor ID: {sensor.Id}, Name: {sensor.Name}");
                cpuFanSensor.Items.Add(sensor);
                pumpFanSensor.Items.Add(sensor);
            }

            // Set the values from the configuration to the combo boxes
            displayType.SelectedIndex = Config.DisplayType;
            temperatureUnit.SelectedIndex = Config.TemperatureUnit;

            cpuTemperatureSensor.SelectedIndex = Config.CpuTemperatureSensor;
            cpuFanSensor.SelectedIndex = Config.CpuFanSensor;
            pumpFanSensor.SelectedIndex = Config.PumpFanSensor;

            // Add event handler for selection change
            displayType.SelectionChanged += SaveConfigHandler;
            temperatureUnit.SelectionChanged += SaveConfigHandler;
            cpuTemperatureSensor.SelectionChanged += SaveConfigHandler;
            cpuFanSensor.SelectionChanged += SaveConfigHandler;
            pumpFanSensor.SelectionChanged += SaveConfigHandler;

            // Set the values for max RPM text boxes
            maxCpuFanRpm.Text = Config.MaxCpuFanRpm.ToString();
            maxPumpFanRpm.Text = Config.MaxPumpFanRpm.ToString();

            // Add event handler for max RPM text boxes
            maxCpuFanRpm.TextChanged += SaveConfigHandler;
            maxPumpFanRpm.TextChanged += SaveConfigHandler;

            maxCpuFanRpm.PreviewTextInput += PreventLettersHandler;
            maxPumpFanRpm.PreviewTextInput += PreventLettersHandler;
        }

        private void ConfigScreen()
        {
            byte[] ef_setArray = new byte[9] { 0, 96, 0, stnLightOnOf_Fan, stnLightOnOf_Pum, stnLightOnOf_Whi, stnLightOnOf_Blu, stnLightOnOf_Lig, 0 };
            classUSBPort.USB_60H(ef_setArray);
        }

        private void UpdatePumpDisplay(int num)
        {
            byte[] array = Utils.UpDisplay(Math.Min(num, 100), Config.DisplayType);
            byte[] ef_setArray = new byte[9] { 0, 98, array[0], array[1], array[2], array[3], array[4], 0, 0 };
            classUSBPort.USB_62H(ef_setArray);
        }

        private void UpdateFanDisplay(int num)
        {
            byte[] array = Utils.UpDisplay(Math.Min(num, 100), Config.DisplayType);
            byte[] ef_setArray = new byte[9] { 0, 97, array[0], array[1], array[2], array[3], array[4], 0, 0 };
            classUSBPort.USB_61H(ef_setArray);
        }

        private void SetCpuTemp(float temp)
        {
            int number1 = 0, number2 = 0, number3 = 0;
            if (Config.TemperatureUnit == 0)
            {
                // Celsius
                string tempStr = Math.Min(temp, 99.9f).ToString("00.0");

                number1 = int.Parse(tempStr[0].ToString());
                number2 = int.Parse(tempStr[1].ToString());
                number3 = int.Parse(tempStr[3].ToString());
            }
            else
            {
                // Fahrenheit
                string tempStr = Utils.celsiusToFahrenheit(temp).ToString("000.0");
                number1 = int.Parse(tempStr[0].ToString());
                number2 = int.Parse(tempStr[1].ToString());
                number3 = int.Parse(tempStr[2].ToString());
            }

            byte byte1 = Utils.NumberToByte(number1);
            byte byte2 = Utils.NumberToByte(number2);
            byte byte3 = Utils.NumberToByte(number3);
            byte byte4 = (byte)(Config.TemperatureUnit == 1 ? 0 : 1);

            byte[] ef_setArray = new byte[9] { 0, 99, byte1, byte2, byte3, byte4, 0, 0, 0 };
            classUSBPort.USB_63H(ef_setArray);
        }

        private void ClearCpuTemp()
        {
            byte[] ef_setArray = new byte[9] { 0, 99, 136, 136, 136, 1, 0, 0, 0 };
            classUSBPort.USB_63H(ef_setArray);
        }

        private void listenHook(string vid, string pid, string port, int plug)
        {
            bool isUsbOnline = UsbOnline();
            bool isDeviceRemoved = isUsbOnline && plug == 0;
            bool isDeviceInserted = !isUsbOnline && plug == 1;
            if (isDeviceRemoved || isDeviceInserted)
            {
                Thread.Sleep(200);
                UsbOnline();
            }
        }
        private bool UsbOnline()
        {
            try
            {
                classUSBOnline.GetOnline();
                classUSBOnline.GetUSB_find(DefaultFindPid);
                bool num = classUSBPort.Init(classUSBOnline.USBHID_bc, DefaultFindPid);
                if (num)
                {
                    Debug.WriteLine("USB Port Path: " + classUSBPort.uSBHID_Port.DevicePath);
                }
                return num;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"[MainWindow] Erro ao inicializar USB: {ex.Message}");
                return false;
            }
        }
        private void SaveConfigHandler(object sender, EventArgs e)
        {
            // Update the configuration based on the selected sensors
            Config.CpuTemperatureSensor = cpuTemperatureSensor.SelectedIndex;
            Config.CpuFanSensor = cpuFanSensor.SelectedIndex;
            Config.PumpFanSensor = pumpFanSensor.SelectedIndex;
            Config.DisplayType = displayType.SelectedIndex;
            Config.TemperatureUnit = temperatureUnit.SelectedIndex;

            // Update the maximum RPM values from the text boxes
            if (int.TryParse(maxCpuFanRpm.Text, out int maxCpuFanRpm_) && int.TryParse(maxPumpFanRpm.Text, out int maxPumpFanRpm_))
            {
                Config.MaxCpuFanRpm = maxCpuFanRpm_;
                Config.MaxPumpFanRpm = maxPumpFanRpm_;
            }

            // Save configuration
            ConfigManager.Save(DefaultConfigPath, Config);

            // Atualiza sensores e reinicia atualização
            sensorUpdater.SetSensorsFromConfig();
        }

        private void PreventLettersHandler(object sender, TextCompositionEventArgs e)
        {
            // Só permite dígitos
            e.Handled = !e.Text.All(char.IsDigit);
        }
    }
}
