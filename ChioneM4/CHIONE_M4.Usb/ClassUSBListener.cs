using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Interop;

namespace CHIONE_M4.Usb;

internal class ClassUSBListener
{
	public delegate void ListenHook(string vid, string pid, string port, int plug);

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	private struct DEV_BROADCAST_DEVICEINTERFACE
	{
		public uint dbcc_size;

		public uint dbcc_devicetype;

		public uint dbcc_reserved;

		public Guid dbcc_classguid;

		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 255)]
		public string dbcc_name;
	}

	private struct DEV_BROADCAST_VOLUME
	{
		public uint dbcv_size;

		public uint dbcv_devicetype;

		public uint dbcv_reserved;

		public uint dbcv_unitmask;

		public ushort dbcv_flags;
	}

	private struct DEV_BROADCAST_HDR
	{
		public uint dbch_size;

		public uint dbch_devicetype;

		public uint dbch_reserved;
	}

	private HwndSource _handleSource;

	private IntPtr _notificationHandle;

	private string OLD_port = "";

	private int OLD_plug;

	public ListenHook listenHook;

	public const int WM_DEVICECHANGE = 537;

	public const int WM_HOTKEY = 786;

	public const int DBT_DEVICEARRIVAL = 32768;

	public const int DBT_DEVICEREMOVECOMPLETE = 32772;

	public const int DBT_DEVNODES_CHANGED = 7;

	private const uint DBT_DEVTYP_VOLUME = 2u;

	private const uint DBT_DEVTYP_DEVICEINTERFACE = 5u;

	private static readonly Guid GUID_DEVINTERFACE_USB_DEVICE = new Guid("A5DCBF10-6530-11D2-901F-00C04FB951ED");

	private const uint DEVICE_NOTIFY_WINDOW_HANDLE = 0u;

	public ClassUSBListener(Window win, ListenHook listenHook)
	{
		_handleSource = PresentationSource.FromVisual(win) as HwndSource;
		this.listenHook = listenHook;
		StartListen();
	}

	public void StartListen()
	{
		if (_handleSource != null)
		{
			_handleSource.AddHook(WndProc);
			Register(_handleSource.Handle);
			Debug.WriteLine("StartListen");
		}
	}

	public void EndListen()
	{
		if (_handleSource != null)
		{
			Unregister();
			_handleSource.RemoveHook(WndProc);
		}
	}

	private void Unregister()
	{
		if (_notificationHandle != IntPtr.Zero)
		{
			UnregisterDeviceNotification(_notificationHandle);
		}
	}

	private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
	{
		if (msg == WM_DEVICECHANGE)
		{
			switch ((int)wParam)
			{
			case 32768:
				HandleMessage(lParam, 1);
				break;
			case 32772:
				HandleMessage(lParam, 0);
				break;
			}
		}
		return IntPtr.Zero;
	}

	private void HandleMessage(IntPtr lParam, int USBDevicePlugType)
	{
		if (((DEV_BROADCAST_HDR)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_HDR))).dbch_devicetype == 5)
		{
			string dbcc_name = ((DEV_BROADCAST_DEVICEINTERFACE)Marshal.PtrToStructure(lParam, typeof(DEV_BROADCAST_DEVICEINTERFACE))).dbcc_name;
			if (!OLD_port.Equals(dbcc_name))
			{
				OLD_port = dbcc_name;
				OLD_plug = USBDevicePlugType;
				USBcallback(dbcc_name, USBDevicePlugType);
			}
			else if (OLD_plug != USBDevicePlugType)
			{
				OLD_plug = USBDevicePlugType;
				USBcallback(dbcc_name, USBDevicePlugType);
			}
			//EndListen();
		}
	}

	private void USBcallback(string path, int USBDevicePlugType)
	{
		string text = Catch_Vid(path);
		string text2 = Catch_Pid(path);
		Debug.WriteLine("vid {0}, pid {1}", text, text2);
		Debug.WriteLine("path {0}, plug {1}", path, USBDevicePlugType);
		listenHook(text, text2, path, USBDevicePlugType);
	}

	public string Catch_Vid(string source)
	{
		Match match = new Regex("VID_[0-9A-Z]{4}", RegexOptions.Compiled).Match(source);
		if (!match.Success)
		{
			return string.Empty;
		}
		string value = match.Value;
		return value.Substring(4, value.Length - 4);
	}

	public string Catch_Pid(string source)
	{
		Match match = new Regex("PID_[0-9A-Z]{4}", RegexOptions.Compiled).Match(source);
		if (!match.Success)
		{
			return string.Empty;
		}
		string value = match.Value;
		return value.Substring(4, value.Length - 4);
	}

	private void Register(IntPtr windowHandle)
	{
		DEV_BROADCAST_DEVICEINTERFACE structure = new DEV_BROADCAST_DEVICEINTERFACE
		{
			dbcc_size = (uint)Marshal.SizeOf(typeof(DEV_BROADCAST_DEVICEINTERFACE)),
			dbcc_devicetype = 5u,
			dbcc_classguid = GUID_DEVINTERFACE_USB_DEVICE
		};
		IntPtr intPtr = Marshal.AllocHGlobal(Marshal.SizeOf(structure));
		Marshal.StructureToPtr(structure, intPtr, fDeleteOld: true);
		_notificationHandle = RegisterDeviceNotification(windowHandle, intPtr, 0u);
		if (_notificationHandle == IntPtr.Zero)
		{
			throw new Win32Exception(Marshal.GetLastWin32Error(), "Failed to register USB device notifications.");
		}
	}

	[DllImport("User32.dll", SetLastError = true)]
	private static extern IntPtr RegisterDeviceNotification(IntPtr hRecipient, IntPtr NotificationFilter, uint Flags);

	[DllImport("User32.dll", SetLastError = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool UnregisterDeviceNotification(IntPtr Handle);
}
