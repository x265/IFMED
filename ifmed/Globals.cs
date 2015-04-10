using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ifmed
{
	class Globals
	{
		static DateTime RetrieveLinkerTimestamp()
		{
			string filePath = System.Reflection.Assembly.GetCallingAssembly().Location;
			const int c_PeHeaderOffset = 60;
			const int c_LinkerTimestampOffset = 8;
			byte[] b = new byte[2048];
			System.IO.Stream s = null;

			try
			{
				s = new System.IO.FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
				s.Read(b, 0, 2048);
			}
			finally
			{
				if (s != null)
				{
					s.Close();
				}
			}

			int i = System.BitConverter.ToInt32(b, c_PeHeaderOffset);
			int secondsSince1970 = System.BitConverter.ToInt32(b, i + c_LinkerTimestampOffset);
			DateTime dt = new DateTime(1970, 1, 1, 0, 0, 0);
			dt = dt.AddSeconds(secondsSince1970);
			dt = dt.AddHours(TimeZone.CurrentTimeZone.GetUtcOffset(dt).Hours);
			return dt;
		}

		public static string BuildDate
		{
			get { return RetrieveLinkerTimestamp().Date.ToString("dd/MMM/yyyy"); }
		}

		public static string BuildVersion
		{
			get { return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(); }
		}

		#if DEBUG
		private const string _Bulid = "DEBUG";
		#else
		private const string _Bulid = "RELEASE";
		#endif
		private static string _CPU = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE");

		public static string CPU
		{
			get
			{
				if (String.IsNullOrEmpty(_CPU))
					return String.Format("{0} {1}", OS.Name, _Bulid);
				else
					return String.Format("{0}/{1} {2}", _CPU, OS.Name, _Bulid);
			}
		}
	}

	class OS
	{
		private static int p = (int)Environment.OSVersion.Platform;
		private static bool _IsWindows = false;
		private static bool _IsLinux = false;

		// Extra note:
		// Windows 7 and 8 return 2
		// Ubuntu 14.04.1 return 4

		public static bool IsWindows
		{
			get
			{
				if (p == 2)
					_IsWindows = true;
				return _IsWindows;
			}
		}

		public static bool IsLinux
		{
			get
			{
				if ((p == 4) || (p == 6) || (p == 128))
					_IsLinux = true;
				return _IsLinux;
			}
		}

		public static string Name
		{
			get
			{
				if ((p == 4) || (p == 6) || (p == 128))
					return "Linux";
				return "Windows";
			}
		}
	}
}
