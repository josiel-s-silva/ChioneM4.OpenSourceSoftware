using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GAMDIAS_only.Product;
using Microsoft.Win32.SafeHandles;

namespace CHIONE_M4.Usb;

internal class ClassUSBPort
{
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

	public class ProPortPath
	{
		public int ID { get; set; }

		public string Path { get; set; }

		public ProPortPath(int ID, string Path)
		{
			this.ID = ID;
			this.Path = Path;
		}
	}

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

	public List<ProPortPath> proPortPaths = new List<ProPortPath>();

	public ClassPROSupport classProSupport = new ClassPROSupport();

	public ClassUSBOnline.USBHID_port uSBHID_Port = new ClassUSBOnline.USBHID_port();

	public bool Pro1_Bool;

	public IntPtr Pro1_IntPtr = IntPtr.Zero;

	public FileStream Pro1_FileStream;

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr CreateFile(string lpFileName, uint dwDesiredAccess, uint dwShareMode, uint lpSecurityAttributes, uint dwCreationDisposition, uint dwFlagsAndAttributes, uint hTemplateFile);

	[DllImport("hid.dll", SetLastError = true)]
	private static extern bool HidD_SetFeature(IntPtr hidDevice, byte[] lpReportBuffer, int ReportBufferLength);

	[DllImport("hid.dll", SetLastError = true)]
	public static extern bool HidD_GetFeature(IntPtr hidDevice, byte[] lpReportBuffer, int ReportBufferLength);

	[DllImport("kernel32.dll", CharSet = CharSet.Auto, ExactSpelling = true, SetLastError = true)]
	internal static extern bool CloseHandle(IntPtr hObject);

	[DllImport("hid.dll")]
	private static extern bool HidD_GetPreparsedData(IntPtr hidDeviceObject, out IntPtr PreparsedData);

	[DllImport("hid.dll")]
	private static extern uint HidP_GetCaps(IntPtr PreparsedData, out HIDP_CAPS Capabilities);

	[DllImport("hid.dll")]
	private static extern bool HidD_FreePreparsedData(IntPtr PreparsedData);

	public byte SCE(byte[] Data)
	{
		byte b = byte.MaxValue;
		foreach (byte b2 in Data)
		{
			b -= b2;
		}
		return b;
	}

	public string B2S(byte[] Buffer)
	{
		return BitConverter.ToString(Buffer).Replace("-", " ");
	}

	public bool Init(List<ClassUSBOnline.USBHID_port> Lusb, string PID)
	{
		proPortPaths = new List<ProPortPath>();
		classProSupport.Support(PID);
		foreach (ClassPROSupport.usbport uSBportDatum in classProSupport.USBportData)
		{
			foreach (ClassUSBOnline.USBHID_port item in Lusb)
			{
				if ((uSBportDatum.InputBufferSize == item._InputBufferSize) & (uSBportDatum.OutputBufferSize == item._OutputBufferSize) & (uSBportDatum.FeaturBeufferSize == item._FeaturBeufferSize) & (uSBportDatum.UsagePagex16 == item.UsagePagex16) & (uSBportDatum.Usagex16 == item.Usagex16))
				{
					proPortPaths.Add(new ProPortPath(uSBportDatum.ID, item.DevicePath));
					uSBHID_Port = item;
				}
			}
		}
		return proPortPaths.Count > 0;
	}

	public bool CreateI(int no = 0)
	{
		int count = proPortPaths.Count;
		if (count == 0)
		{
			return false;
		}
		if (no > count)
		{
			return false;
		}
		string path = proPortPaths[no].Path;
		Pro1_IntPtr = CreateFile(path, 3221225472u, 3u, 0u, 3u, 1073741952u, 0u);
		if (Pro1_IntPtr == IntPtr.Zero || (int)Pro1_IntPtr == -1)
		{
			Pro1_Bool = false;
			CloseHandle(Pro1_IntPtr);
		}
		else
		{
			HidD_GetPreparsedData(Pro1_IntPtr, out var PreparsedData);
			HidP_GetCaps(PreparsedData, out var Capabilities);
			HidD_FreePreparsedData(PreparsedData);
			Pro1_Bool = true;
			Pro1_FileStream = new FileStream(new SafeFileHandle(Pro1_IntPtr, ownsHandle: true), FileAccess.ReadWrite, Math.Max(Capabilities.OutputReportByteLength, Capabilities.InputReportByteLength), isAsync: true);
		}
		return Pro1_Bool;
	}

	public void CloseI()
	{
		try
		{
			if (Pro1_Bool && Pro1_FileStream != null)
			{
				Pro1_FileStream.Dispose();
				CloseHandle(Pro1_IntPtr);
				Pro1_Bool = false;
			}
		}
		catch
		{
		}
	}

	public void SetLIVE1Feature(byte[] data)
	{
		if (Pro1_Bool)
		{
			HidD_SetFeature(Pro1_IntPtr, data, data.Length);
		}
	}

	public byte[] GetLIVE1Feature(byte[] data)
	{
		if (!Pro1_Bool)
		{
			return new byte[data.Length];
		}
		int num = data.Length;
		byte[] array = new byte[num];
		HidD_SetFeature(Pro1_IntPtr, data, num);
		HidD_GetFeature(Pro1_IntPtr, array, num);
		return array;
	}

	public byte[] USB_30H()
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		byte[] data = new byte[9] { 0, 48, 0, 0, 0, 0, 0, 0, 207 };
		return GetLIVE1Feature(data);
	}

	public byte[] USB_40H(byte[] Ef_setArray)
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		Ef_setArray[8] = SCE(Ef_setArray);
		return GetLIVE1Feature(Ef_setArray);
	}

	public byte[] USB_50H(byte[] Ef_setArray)
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		Ef_setArray[8] = SCE(Ef_setArray);
		return GetLIVE1Feature(Ef_setArray);
	}

	public byte[] USB_41H(byte[] Ef_setArray)
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		Ef_setArray[8] = SCE(Ef_setArray);
		return GetLIVE1Feature(Ef_setArray);
	}

	public byte[] USB_51H(byte[] Ef_setArray)
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		Ef_setArray[8] = SCE(Ef_setArray);
		return GetLIVE1Feature(Ef_setArray);
	}

	public byte[] USB_42H(byte[] Ef_setArray)
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		Ef_setArray[8] = SCE(Ef_setArray);
		return GetLIVE1Feature(Ef_setArray);
	}

	public byte[] USB_52H(byte[] Ef_setArray)
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		Ef_setArray[8] = SCE(Ef_setArray);
		return GetLIVE1Feature(Ef_setArray);
	}

	public byte[] USB_60H(byte[] Ef_setArray)
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		Ef_setArray[8] = SCE(Ef_setArray);
		return GetLIVE1Feature(Ef_setArray);
	}

	public byte[] USB_70H(byte[] Ef_setArray)
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		Ef_setArray[8] = SCE(Ef_setArray);
		return GetLIVE1Feature(Ef_setArray);
	}

	public byte[] USB_61H(byte[] Ef_setArray)
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		Ef_setArray[8] = SCE(Ef_setArray);
		return GetLIVE1Feature(Ef_setArray);
	}

	public byte[] USB_62H(byte[] Ef_setArray)
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		Ef_setArray[8] = SCE(Ef_setArray);
		return GetLIVE1Feature(Ef_setArray);
	}

	public byte[] USB_71H(byte[] Ef_setArray)
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		Ef_setArray[8] = SCE(Ef_setArray);
		return GetLIVE1Feature(Ef_setArray);
	}

	public byte[] USB_72H(byte[] Ef_setArray)
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		Ef_setArray[8] = SCE(Ef_setArray);
		return GetLIVE1Feature(Ef_setArray);
	}

	public byte[] USB_63H(byte[] Ef_setArray)
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		Ef_setArray[8] = SCE(Ef_setArray);
		return GetLIVE1Feature(Ef_setArray);
	}

	public byte[] USB_73H(byte[] Ef_setArray)
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		Ef_setArray[8] = SCE(Ef_setArray);
		return GetLIVE1Feature(Ef_setArray);
	}

	public byte[] USB_80H(byte[] Ef_setArray)
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		Ef_setArray[8] = SCE(Ef_setArray);
		return GetLIVE1Feature(Ef_setArray);
	}

	public async void USB_80HBuffer(List<List<byte>> listbyte)
	{
		if (Pro1_FileStream == null)
		{
			return;
		}
		foreach (List<byte> item in listbyte)
		{
			await Pro1_FileStream.WriteAsync(item.ToArray(), 0, 65);
			await Task.Delay(10);
		}
	}

	public byte[] USB_90H(byte[] Ef_setArray)
	{
		if (!CreateI())
		{
			return new byte[0];
		}
		Ef_setArray[8] = SCE(Ef_setArray);
		return GetLIVE1Feature(Ef_setArray);
	}
}
