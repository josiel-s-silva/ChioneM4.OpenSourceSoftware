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
        private string find_PID = "B533";

        private AppConfig Config;

        private ClassUSBListener classUSBListener;
        private ClassUSBOnline classUSBOnline;
        private ClassUSBPort classUSBPort = new ClassUSBPort();

        private MonitorManager monitorManager = new MonitorManager();
        private Thread refreshThread;
        private bool canRestartRefreshThread = false;

        private byte stnLightOnOf_Fan = 1;
        private byte stnLightOnOf_Pum = 1;
        private byte stnLightOnOf_Whi = 1;
        private byte stnLightOnOf_Blu = 1;
        private byte stnLightOnOf_Lig = 1;
        public MainWindow()
        {
            InitializeComponent();

            // Load configuration
            Config = ConfigManager.Load("config.json");

            // Start the USB listener
            classUSBListener = new ClassUSBListener(this, listenHook);
            classUSBOnline = new ClassUSBOnline();
            UsbOnline();

            // Config water cooler display
            ConfigScreen();

            // Set the initial values for the text boxes
            loadUI();

            // Start the refresh thread
            StartupRefreshThread();
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            // Clear display
            ClearCpuTemp();
            UpdateFanDisplay(0);
            UpdatePumpDisplay(0);

            // Stop the refresh thread if it's running
            ShutdownRefreshThread();
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
            cpuTemperatureSensor.SelectedIndex = Config.CpuTemperatureSensor;
            cpuFanSensor.SelectedIndex = Config.CpuFanSensor;
            pumpFanSensor.SelectedIndex = Config.PumpFanSensor;

            // Add event handler for selection change
            cpuTemperatureSensor.SelectionChanged += ComboBox_SelectionChanged;
            cpuFanSensor.SelectionChanged += ComboBox_SelectionChanged;
            pumpFanSensor.SelectionChanged += ComboBox_SelectionChanged;

            // Set the values for max RPM text boxes
            maxCpuFanRpm.Text = Config.MaxCpuFanRpm.ToString();
            maxPumpFanRpm.Text = Config.MaxPumpFanRpm.ToString();

            // Add event handler for max RPM text boxes
            maxCpuFanRpm.TextChanged += TextBox_TextChanged;
            maxPumpFanRpm.TextChanged += TextBox_TextChanged;

            maxCpuFanRpm.PreviewTextInput += TextBox_PreviewTextInput;
            maxPumpFanRpm.PreviewTextInput += TextBox_PreviewTextInput;
        }

        private void StartupRefreshThread()
        {
            SensorInfo selectedCpuTemperatureSensor = (SensorInfo)cpuTemperatureSensor.SelectedItem;
            SensorInfo selectedCpuFanSensor = (SensorInfo)cpuFanSensor.SelectedItem;
            SensorInfo selectedPumpFanSensor = (SensorInfo)pumpFanSensor.SelectedItem;

            refreshThread = new Thread(() =>
            {
                while (true)
                {
                    selectedCpuTemperatureSensor.Sensor.Hardware.Update();
                    selectedCpuFanSensor.Sensor.Hardware.Update();
                    selectedPumpFanSensor.Sensor.Hardware.Update();

                    float cpuTemp = selectedCpuTemperatureSensor.Sensor.Value ?? 0;

                    float cpuFan = selectedCpuFanSensor.Sensor.Value ?? 0;
                    float pumpFan = selectedPumpFanSensor.Sensor.Value ?? 0;

                    int cpuFanPorcentage = (int)(cpuFan / Config.MaxCpuFanRpm * 100);
                    int pumpFanPorcentage = (int)(pumpFan / Config.MaxPumpFanRpm * 100);

                    Debug.WriteLine($"CPU Temp: {cpuTemp} °C, CPU Fan: {cpuFan} RPM, Pump Fan: {pumpFan} RPM");
                    Debug.WriteLine($"CPU Fan Porcentage: {cpuFanPorcentage} %, Pump Fan Porcentage: {pumpFanPorcentage} %");

                    SetCpuTemp(cpuTemp);
                    UpdateFanDisplay(cpuFanPorcentage);
                    UpdatePumpDisplay(pumpFanPorcentage);

                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        cpuTemperatureLabel.Content = $"{cpuTemp:00.0} °C";
                        cpuFanLabel.Content = $"{cpuFan:00.0} RPM";
                        pumpFanLabel.Content = $"{pumpFan:00.0} RPM";
                    });

                    Thread.Sleep(1000);
                }
            });
            refreshThread.Start();
        }

        private void ShutdownRefreshThread()
        {
            if (refreshThread != null && refreshThread.IsAlive)
            {
                refreshThread.Abort();
            }
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
            string tempStr = Math.Min(temp, 99.9f).ToString("00.0");

            int number1 = int.Parse(tempStr[0].ToString());
            int number2 = int.Parse(tempStr[1].ToString());
            int number3 = int.Parse(tempStr[3].ToString());

            byte byte1 = Utils.NumberToByte(number1);
            byte byte2 = Utils.NumberToByte(number2);
            byte byte3 = Utils.NumberToByte(number3);
            byte byte4 = 1;

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
            classUSBOnline.GetOnline();
            classUSBOnline.GetUSB_find(find_PID);
            bool num = classUSBPort.Init(classUSBOnline.USBHID_bc, find_PID);
            if (num)
            {
                Debug.WriteLine("USB Port Path: " + classUSBPort.uSBHID_Port.DevicePath);
            }
            return num;
        }
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Update the configuration based on the selected sensors
            Config.CpuTemperatureSensor = cpuTemperatureSensor.SelectedIndex;
            Config.CpuFanSensor = cpuFanSensor.SelectedIndex;
            Config.PumpFanSensor = pumpFanSensor.SelectedIndex;

            // Save configuration when a sensor is changed
            ConfigManager.Save("config.json", Config);

            // Restart the refresh thread to apply the new sensor selection
            ShutdownRefreshThread();
            StartupRefreshThread();
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (int.TryParse(maxCpuFanRpm.Text, out int maxCpuFanRpm_) && int.TryParse(maxPumpFanRpm.Text, out int maxPumpFanRpm_))
            {
                Config.MaxCpuFanRpm = maxCpuFanRpm_;
                Config.MaxPumpFanRpm = maxPumpFanRpm_;

                // Save configuration when max RPM is changed
                ConfigManager.Save("config.json", Config);

                // Restart the refresh thread to apply the new max RPM values
                ShutdownRefreshThread();
                StartupRefreshThread();
            }
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Só permite dígitos
            e.Handled = !e.Text.All(char.IsDigit);
        }
    }
}
