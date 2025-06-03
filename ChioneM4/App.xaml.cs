using System;
using System.Linq;
using System.Windows;
using System.Drawing;

namespace WaterCoolerM4
{
    /// <summary>
    /// Interação lógica para App.xaml
    /// </summary>
    public partial class App : Application
    {
        private System.Windows.Forms.NotifyIcon notifyIcon;
        private MainWindow mainWindow;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            bool minimized = e.Args.Contains("--minimized");

            mainWindow = new MainWindow();
            mainWindow.StateChanged += MainWindow_StateChanged;
            mainWindow.Closed += MainWindow_Closed;

            notifyIcon = new System.Windows.Forms.NotifyIcon();
            notifyIcon.Icon = Icon.ExtractAssociatedIcon(System.Windows.Forms.Application.ExecutablePath);
            notifyIcon.Visible = minimized;
            notifyIcon.Text = "WaterCoolerM4";
            notifyIcon.DoubleClick += NotifyIcon_DoubleClick;

            if (!minimized)
            {
                mainWindow.Show();
            }
        }

        private void MainWindow_StateChanged(object sender, EventArgs e)
        {
            if (mainWindow.WindowState == WindowState.Minimized)
            {
                mainWindow.Hide();
                notifyIcon.Visible = true;
            }
            else if (mainWindow.WindowState == WindowState.Normal)
            {
                notifyIcon.Visible = false;
            }
        }

        private void NotifyIcon_DoubleClick(object sender, EventArgs e)
        {
            mainWindow.Show();
            mainWindow.WindowState = WindowState.Normal;
            mainWindow.Activate();
        }

        private void MainWindow_Closed(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
        }
    }
}
