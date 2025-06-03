using System.Collections.Generic;

namespace GAMDIAS_only.Product;

public class ClassPROSupport
{
	public class pro
	{
		public string type { get; set; }

		public string pid { get; set; }

		public string name { get; set; }

		public string likepid { get; set; }

		public List<usbport> USBportData { get; set; }

		public pro(string type, string pid, string name, string like, List<usbport> USBportData)
		{
			this.type = type;
			this.pid = pid;
			this.name = name;
			likepid = like;
			this.USBportData = USBportData;
		}
	}

	public class usbport
	{
		public int ID { get; set; }

		public int InputBufferSize { get; set; }

		public int OutputBufferSize { get; set; }

		public int FeaturBeufferSize { get; set; }

		public string UsagePagex16 { get; set; }

		public string Usagex16 { get; set; }

		public usbport()
		{
		}

		public usbport(int ID, int _InputBufferSize, int _OutputBufferSize, int _FeatureufferSize, string UsagePagex16, string Usagex16)
		{
			this.ID = ID;
			InputBufferSize = _InputBufferSize;
			OutputBufferSize = _OutputBufferSize;
			FeaturBeufferSize = _FeatureufferSize;
			this.UsagePagex16 = UsagePagex16;
			this.Usagex16 = Usagex16;
		}
	}

	public List<pro> Listpro;

	public string type { get; set; }

	public string name { get; set; }

	public bool supbool { get; set; }

	public string likepid { get; set; }

	public List<usbport> USBportData { get; set; }

	public ClassPROSupport()
	{
		Init();
	}

	private void Init()
	{
		Listpro = new List<pro>();
		Listpro.Add(new pro("STN", "B533", "USB-STN", "", new List<usbport>
		{
			new usbport(0, 65, 65, 9, "0xFF02", "0x0001")
		}));
		Listpro.Add(new pro("Box", "B522", "USB-LCD", "", new List<usbport>
		{
			new usbport(0, 8, 1025, 9, "0xFF02", "0x0001")
		}));
		Listpro.Add(new pro("Key", "B526", "HETMES P1A", "", new List<usbport>
		{
			new usbport(0, 65, 65, 9, "0xFF01", "0x0001"),
			new usbport(1, 4, 0, 0, "0xFF02", "0x0001")
		}));
		Listpro.Add(new pro("Mouse", "B519", "ZEUS XX", "B520", new List<usbport>
		{
			new usbport(),
			new usbport()
		}));
	}

	public void Support(string pid)
	{
		pid = pid.ToUpper();
		type = "";
		name = "";
		supbool = false;
		likepid = "";
		foreach (pro item in Listpro)
		{
			if (item.pid.Equals(pid))
			{
				type = item.type;
				name = item.name;
				supbool = true;
				likepid = item.likepid;
				USBportData = item.USBportData;
			}
		}
	}
}
