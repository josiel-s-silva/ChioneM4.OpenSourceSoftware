using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using GAMDIAS_only.Product;
using Microsoft.Win32;

namespace CHIONE_M4.Usb;

internal class ClassUSBOnline
{
	public class USBHID_port
	{
		private int First = -1;

		private int End = -1;

		public string VID { get; set; }

		public string PID { get; set; }

		public string PIDName { get; set; }

		public string PIDType { get; set; }

		public string DevicePath { get; set; }

		public string FimwareVer { get; set; }

		public string ContainerID { get; set; }

		public int _InputBufferSize { get; set; }

		public int _OutputBufferSize { get; set; }

		public int _FeaturBeufferSize { get; set; }

		public string UsagePagex16 { get; set; }

		public string Usagex16 { get; set; }

		public USBHID_port()
		{
		}

		public USBHID_port(string VID, string PID, string PIDName, string PIDType, string DevicePath, string FimwareVer, string ContainerID, int _InputBufferSize, int _OutputBufferSize, int _FeatureufferSize, string UsagePagex16, string Usagex16)
		{
			this.VID = VID;
			this.PID = PID;
			this.PIDName = PIDName;
			this.PIDType = PIDType;
			this.DevicePath = DevicePath;
			this.FimwareVer = FimwareVer;
			this.ContainerID = ContainerID;
			this._InputBufferSize = _InputBufferSize;
			this._OutputBufferSize = _OutputBufferSize;
			_FeaturBeufferSize = _FeatureufferSize;
			this.UsagePagex16 = UsagePagex16;
			this.Usagex16 = Usagex16;
		}
	}

	private enum DIGCF
	{
		DIGCF_DEFAULT = 1,
		DIGCF_PRESENT = 2,
		DIGCF_ALLCLASSES = 4,
		DIGCF_PROFILE = 8,
		DIGCF_DEVICEINTERFACE = 0x10
	}

	private struct SP_DEVICE_INTERFACE_DATA
	{
		public int cbSize;

		public Guid interfaceClassGuid;

		public int flags;

		public int reserved;
	}

	[StructLayout(LayoutKind.Sequential)]
	private class SP_DEVINFO_DATA
	{
		public int cbSize = Marshal.SizeOf<SP_DEVINFO_DATA>();

		public Guid classGuid = Guid.Empty;

		public int devInst;

		public int reserved;
	}

	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	private struct SP_DEVICE_INTERFACE_DETAIL_DATA
	{
		internal int cbSize;

		internal short devicePath;
	}

	private struct HID_ATTRIBUTES
	{
		public int Size;

		public ushort VendorID;

		public ushort ProductID;

		public ushort VersionNumber;
	}

	private struct HIDP_CAPS
	{
		public ushort Usage;

		public ushort UsagePage;

		public ushort InputReportByteLength;

		public ushort OutputReportByteLength;

		public ushort FeatureReportByteLength;

		[MarshalAs(UnmanagedType.ByValArray, SizeConst = 17)]
		public ushort[] Reserved;

		public ushort NumberLinkCollectionNodes;

		public ushort NumberInputButtonCaps;

		public ushort NumberInputValueCaps;

		public ushort NumberInputDataIndices;

		public ushort NumberOutputButtonCaps;

		public ushort NumberOutputValueCaps;

		public ushort NumberOutputDataIndices;

		public ushort NumberFeatureButtonCaps;

		public ushort NumberFeatureValueCaps;

		public ushort NumberFeatureDataIndices;
	}

	private string GAM_VID = "1b80";

	public List<USBHID_port> USBHID_port_List = new List<USBHID_port>();

	public List<USBHID_port> USBHID_bc = new List<USBHID_port>();

	public int cID_bc;

	public ClassPROSupport classProSupport = new ClassPROSupport();

	private RegistryKey Key;

	private string path = "HKEY_LOCAL_MACHINE\\SYSTEM\\CurrentControlSet\\Enum\\HID\\";

	private const short INVALID_HANDLE_VALUE = -1;

	private const uint GENERIC_READ = 2147483648u;

	private const uint GENERIC_WRITE = 1073741824u;

	private const uint FILE_SHARE_READ = 1u;

	private const uint FILE_SHARE_WRITE = 2u;

	private const uint CREATE_NEW = 1u;

	private const uint CREATE_ALWAYS = 2u;

	private const uint OPEN_EXISTING = 3u;

	private const uint FILE_FLAG_OVERLAPPED = 1073741824u;

	private const uint FILE_ATTRIBUTE_NORMAL = 128u;

	public string VID { get; set; }

	public string PID { get; set; }

	public string FimwareVer { get; set; }

	public string ContainerID { get; set; }

	public int _InputBufferSize { get; set; }

	public int _OutputBufferSize { get; set; }

	public int _FeatureufferSize { get; set; }

	public string UsagePagex16 { get; set; }

	public string Usagex16 { get; set; }

	public void GetOnline()
	{
		USBHID_port_List = new List<USBHID_port>();
		Guid HidGuid = Guid.Empty;
		HidD_GetHidGuid(ref HidGuid);
		IntPtr intPtr = SetupDiGetClassDevs(ref HidGuid, 0u, IntPtr.Zero, (DIGCF)18);
		if (intPtr == IntPtr.Zero)
		{
			return;
		}
		SP_DEVICE_INTERFACE_DATA deviceInterfaceData = default(SP_DEVICE_INTERFACE_DATA);
		deviceInterfaceData.cbSize = Marshal.SizeOf(deviceInterfaceData);
		uint num = 0u;
		int NotSupInt = 0;
		string DevicePath = "";
		for (; SetupDiEnumDeviceInterfaces(intPtr, IntPtr.Zero, ref HidGuid, num, ref deviceInterfaceData); num++)
		{
			int requiredSize = 0;
			new SP_DEVINFO_DATA();
			SetupDiGetDeviceInterfaceDetail(intPtr, ref deviceInterfaceData, IntPtr.Zero, 0, ref requiredSize, null);
			IntPtr intPtr2 = Marshal.AllocHGlobal(requiredSize);
			Marshal.StructureToPtr(new SP_DEVICE_INTERFACE_DETAIL_DATA
			{
				cbSize = Marshal.SizeOf(typeof(SP_DEVICE_INTERFACE_DETAIL_DATA))
			}, intPtr2, fDeleteOld: false);
			if (SetupDiGetDeviceInterfaceDetail(intPtr, ref deviceInterfaceData, intPtr2, requiredSize, ref requiredSize, null))
			{
				DevicePath = Marshal.PtrToStringAuto(IntPtr.Add(intPtr2, 4));
				if (DevicePath.IndexOf(GAM_VID) != -1)
				{
					AddUsbPort();
				}
			}
			Marshal.FreeHGlobal(intPtr2);
		}
		IntPtr USB_FileHandle = IntPtr.Zero;
		void AddUsbPort()
		{
			IntPtr intPtr3 = USB_CreateFile(DevicePath, 0);
			if (!(intPtr3 == IntPtr.Zero) && (int)intPtr3 != -1)
			{
				HidD_GetAttributes(intPtr3, out var attributes);
				HidD_GetPreparsedData(intPtr3, out var PreparsedData);
				HidP_GetCaps(PreparsedData, out var Capabilities);
				HidD_FreePreparsedData(PreparsedData);
				USBHID_init();
				ushort versionNumber = attributes.VersionNumber;
				int num2 = versionNumber / 256;
				int num3 = versionNumber % 256;
				FimwareVer = Convert.ToString(num2, 16) + "." + Convert.ToString(num3, 16);
				_InputBufferSize = Capabilities.InputReportByteLength;
				_OutputBufferSize = Capabilities.OutputReportByteLength;
				_FeatureufferSize = Capabilities.FeatureReportByteLength;
				UsagePagex16 = $"0x{Capabilities.UsagePage:X4}";
				Usagex16 = $"0x{Capabilities.Usage:X4}";
				RegeditFindClassGUID(DevicePath);
				classProSupport.Support(PID);
				if (classProSupport.supbool)
				{
					USBHID_port_List.Add(new USBHID_port(VID, PID, classProSupport.name, classProSupport.type, DevicePath, FimwareVer, ContainerID, _InputBufferSize, _OutputBufferSize, _FeatureufferSize, UsagePagex16, Usagex16));
				}
				else
				{
					NotSupInt++;
				}
			}
			CloseHandle(intPtr3);
		}
		IntPtr USB_CreateFile(string USB_DevicePath, int Mode)
		{
			USB_FileHandle = CreateFile(USB_DevicePath, 3221225472u, 3u, 0u, 3u, 1073741952u, 0u);
			return USB_FileHandle;
		}
	}

	public void GetUSB_find(string pid)
	{
		USBHID_bc = new List<USBHID_port>();
		cID_bc = 0;
		if (USBHID_port_List.Count == 0)
		{
			return;
		}
		USBHID_port_List = (from o in USBHID_port_List
			orderby o.PID, o.ContainerID
			select o).ToList();
		foreach (USBHID_port uSBHID_port_ in USBHID_port_List)
		{
			Debug.WriteLine("GetUSB_find {0} HID.PID {1} pid {2}", uSBHID_port_.PID == pid, uSBHID_port_.PID, pid);
			if (uSBHID_port_.PID == pid.ToLower())
			{
				USBHID_bc.Add(uSBHID_port_);
			}
		}
	}

	private void USBHID_init()
	{
		VID = "";
		PID = "";
		FimwareVer = "";
		ContainerID = "";
		_InputBufferSize = -1;
		_OutputBufferSize = -1;
		_FeatureufferSize = -1;
		UsagePagex16 = "";
		Usagex16 = "";
	}

	private void RegeditFindClassGUID(string Upath)
	{
		try
		{
			string text = "";
			if (Upath.IndexOf("mi_") > -1)
			{
				string text2 = Upath.Substring(Upath.IndexOf("vid_") + 4, 4).ToUpper();
				string text3 = Upath.Substring(Upath.IndexOf("pid_") + 4, 4).ToUpper();
				string text4 = Upath.Substring(Upath.IndexOf("mi_") + 3, 2);
				string text5 = Upath.Substring(Upath.IndexOf("mi_") + 6, Upath.IndexOf("#{") - Upath.IndexOf("mi_") - 6);
				text = "VID_" + text2 + "&PID_" + text3 + "&MI_" + text4 + "\\" + text5;
				VID = text2;
				PID = text3.ToLower();
			}
			if (Upath.IndexOf("col") > -1)
			{
				string text6 = Upath.Substring(Upath.IndexOf("vid_") + 4, 4).ToUpper();
				string text7 = Upath.Substring(Upath.IndexOf("pid_") + 4, 4).ToUpper();
				string text8 = Upath.Substring(Upath.IndexOf("mi_") + 3, 2);
				string text9 = Upath.Substring(Upath.IndexOf("col") + 3, 2);
				string text10 = Upath.Substring(Upath.IndexOf("col") + 6, Upath.IndexOf("#{") - Upath.IndexOf("col") - 6);
				text = "VID_" + text6 + "&PID_" + text7 + "&MI_" + text8 + "&Col" + text9 + "\\" + text10;
				VID = text6;
				PID = text7.ToLower();
			}
			if ((Upath.IndexOf("mi_") == -1) & (Upath.IndexOf("col") == -1))
			{
				string text11 = Upath.Substring(Upath.IndexOf("vid_") + 4, 4).ToUpper();
				string text12 = Upath.Substring(Upath.IndexOf("pid_") + 4, 4).ToUpper();
				string text13 = Upath.Substring(Upath.IndexOf(text12) + 5, Upath.IndexOf("#{") - Upath.IndexOf(text12) - 5);
				text13 = Upath.Substring(Upath.IndexOf("pid_") + 9, Upath.IndexOf("#{") - Upath.IndexOf("pid_") - 9);
				text = "VID_" + text11 + "&PID_" + text12 + "\\" + text13;
				VID = text11;
				PID = text12.ToLower();
			}
			path = "SYSTEM\\CurrentControlSet\\Enum\\HID\\" + text;
			string containerID = "";
			Key = Registry.LocalMachine.OpenSubKey(path);
			if (Key != null)
			{
				if (Key.GetValue("ContainerID") != null)
				{
					containerID = Key.GetValue("ContainerID").ToString();
				}
				Key.Close();
				ContainerID = containerID;
			}
		}
		catch
		{
			ContainerID = "";
		}
	}

	[DllImport("hid.dll")]
	private static extern void HidD_GetHidGuid(ref Guid HidGuid);

	[DllImport("setupapi.dll", SetLastError = true)]
	private static extern IntPtr SetupDiGetClassDevs(ref Guid ClassGuid, uint Enumerator, IntPtr HwndParent, DIGCF Flags);

	[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool SetupDiEnumDeviceInterfaces(IntPtr hDevInfo, IntPtr devInfo, ref Guid interfaceClassGuid, uint memberIndex, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData);

	[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool SetupDiGetDeviceInterfaceDetail(IntPtr deviceInfoSet, ref SP_DEVICE_INTERFACE_DATA deviceInterfaceData, IntPtr deviceInterfaceDetailData, int deviceInterfaceDetailDataSize, ref int requiredSize, SP_DEVINFO_DATA deviceInfoData);

	[DllImport("setupapi.dll", CharSet = CharSet.Auto, SetLastError = true)]
	private static extern bool SetupDiDestroyDeviceInfoList(IntPtr HIDInfoSet);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, uint lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, uint hTemplateFile);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
	internal static extern bool CloseHandle(IntPtr hObject);

	[DllImport("hid.dll")]
	private static extern bool HidD_GetAttributes(IntPtr hidDevice, out HID_ATTRIBUTES attributes);

	[DllImport("hid.dll")]
	private static extern bool HidD_GetPreparsedData(IntPtr hidDeviceObject, out IntPtr PreparsedData);

	[DllImport("hid.dll")]
	private static extern uint HidP_GetCaps(IntPtr PreparsedData, out HIDP_CAPS Capabilities);

	[DllImport("hid.dll")]
	private static extern bool HidD_FreePreparsedData(IntPtr PreparsedData);

	[DllImport("hid.dll", SetLastError = true)]
	private static extern bool HidD_SetFeature(IntPtr hidDevice, byte[] lpReportBuffer, int ReportBufferLength);

	[DllImport("hid.dll", SetLastError = true)]
	public static extern bool HidD_GetFeature(IntPtr hidDevice, byte[] lpReportBuffer, int ReportBufferLength);

	[DllImport("hid.dll", SetLastError = true)]
	public static extern bool HidD_GetInputReport(IntPtr hidDevice, byte[] lpReportBuffer, int ReportBufferLength);

	[DllImport("hid.dll", SetLastError = true)]
	public static extern bool HidD_GetSerialNumberString(IntPtr hidDevice, byte[] lpReportBuffer, int ReportBufferLength);

	[DllImport("kernel32", SetLastError = true)]
	private static extern bool ReadFile(IntPtr hFile, byte[] lpBuffer, int NumberOfBytesToRead, int pNumberOfBytesRead, int Overlapped);
}
