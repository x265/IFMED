using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace ifmed
{
	class Program
	{
		static void Main(string[] args)
		{
			bool server = false;
			bool client = false;
			bool isPipe = false;
			IPAddress host = IPAddress.Any;
			int port = 4000;

			Head();

			// No help
			if (args.Count() == 0)
				Help(0);
			
			// Get all command-line
			for (int i = 0; i < args.Count(); i++)
			{
				if (args[i].ToLower() == "--help")
				{
					Help(99);
				}

				if (args[i].ToLower() == "-s")
				{
					server = true;
					client = false;
				}

				if (args[i].ToLower() == "-c")
				{
					client = true;
					server = false;
				}

				if (args[i].ToLower() == "-h")
				{
					if (IPAddress.TryParse(args[i + i], out host))
						continue;
					else
						Help(1);
				}

				if (args[i].ToLower() == "-p")
				{
					if (int.TryParse(args[i + i], out port))
						continue;
					else
						Help(2);
				}

				if (args[i].ToLower() == "-")
				{
					isPipe = true;
					continue;
				}
			}

			// Make sure is a valid port
			if (port < 1024 || port > 65535)
				Help(2);

			// Make sure having pipe
			if (!isPipe)
				Help(3);

			// Server mode
			if (server && !client)
			{
				try
				{
					Console.Title = "IFME Network: Server Mode";

					TcpListener Listen = new TcpListener(host, port);
					Listen.Start();

					PrintF(0, "IP : " + Listen.LocalEndpoint);
					PrintF(0, "Awaiting Connection...");

					Socket Sock = Listen.AcceptSocket();

					PrintF(1, "Connected! IP: " + Sock.RemoteEndPoint);

					using (Stream stdout = Console.OpenStandardOutput())
					{
						byte[] buffer = new byte[2048];
						int bytes;
						while ((bytes = Sock.Receive(buffer)) > 0)
						{
							stdout.Write(buffer, 0, bytes);
						}
					}

					Sock.Close();
					Listen.Stop();
				}
				catch (Exception e)
				{
					PrintF(3, e.StackTrace);
				}
			}

			// Client Mode
			if (!server && client)
			{
				try
				{
					Console.Title = "IFME Network: Client Mode";

					TcpClient Client = new TcpClient();

					if (host == IPAddress.Any)
						host = IPAddress.Parse("127.0.0.1");

					PrintF(0, "Connecting to " + host + ":" + port);
					Client.Connect(host, port);

					PrintF(1, "Connected!");
					Stream Raw = Client.GetStream();

					PrintF(0, "Sending Data!");
					using (Stream stdin = Console.OpenStandardInput())
					{
						byte[] buffer = new byte[2048];
						int bytes;
						while ((bytes = stdin.Read(buffer, 0, buffer.Length)) > 0)
						{
							Raw.Write(buffer, 0, bytes);
						}
					}

					PrintF(1, "Data sent successfully! Connection closed.");
					Client.Close();
				}
				catch (Exception e)
				{
					PrintF(3, e.StackTrace);
				}
			}
		}

		static void PrintF(int code, string text)
		{
			switch (code)
			{
				case 0:
					Console.Error.Write("ifme [");
					Console.ForegroundColor = ConsoleColor.Cyan;
					Console.Error.Write("info");
					Console.ResetColor();
					Console.Error.Write("]: {0}\n", text);
					break;
				case 1:
					Console.Error.Write("ifme [");
					Console.ForegroundColor = ConsoleColor.Green;
					Console.Error.Write(" ok ");
					Console.ResetColor();
					Console.Error.Write("]: {0}\n", text);
					break;
				case 2:
					Console.Error.Write("ifme [");
					Console.ForegroundColor = ConsoleColor.Yellow;
					Console.Error.Write("warn");
					Console.ResetColor();
					Console.Error.Write("]: {0}\n", text);
					break;
				case 3:
					Console.Error.Write("ifme [");
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Error.Write("erro");
					Console.ResetColor();
					Console.Error.Write("]: {0}\n", text);
					break;
				default:
					break;
			}
		}

		static void Head()
		{
			Console.ForegroundColor = ConsoleColor.Yellow;
			Console.Error.WriteLine("Internet Friendly Media Encoder Distributed - by Anime4000 (x265.github.io)");
			Console.Error.WriteLine("Version: {0} compiled on {1} ({2})\n", Globals.BuildVersion, Globals.BuildDate, Globals.CPU);
			Console.ResetColor();
		}

		static void Help(int code)
		{
			Console.Error.WriteLine("Lets you encode video over network, splits frames for each server");
			Console.Error.WriteLine("and equally distributes jobs to available workers in parallel.");
			Console.Error.WriteLine();
			Console.Error.WriteLine("Usage: ifmed [--help] [-s/-c] [-h ip] [-p port] [< \"nope.avi\"]");
			Console.Error.WriteLine("       ifmed [--help] [-s/-c] [-h ip] [-p port] [| ffmpeg.exe]");
			Console.Error.WriteLine();
			Console.Error.WriteLine("Options:");
			Console.Error.WriteLine("   --help   Display help");
			Console.Error.WriteLine("   -s       Run as server");
			Console.Error.WriteLine("   -c       Run as client");
			Console.Error.WriteLine("   -h       IP Address (Default: 127.0.0.1/All interface)");
			Console.Error.WriteLine("   -p       TCP Port (Range: 1024 - 65535. Default: 4000)");
			Console.Error.WriteLine("   -        stdin/stdout");
			Console.Error.WriteLine();
			Console.Error.WriteLine("Example:");
			Console.Error.WriteLine("   Client mode, distribute stream");
			Console.Error.WriteLine("   ifmed -c -h 192.168.1.2 - < \"nope.avi\"");
			Console.Error.WriteLine("   ifmed -c -h 192.168.1.2 -p 4001 - < \"nope.avi\"");
			Console.Error.WriteLine("   avs2pipe video \"nope.avs\" | ifmed -c -h 192.168.1.2 -");
			Console.Error.WriteLine();
			Console.Error.WriteLine("   Server mode, encode stream");
			Console.Error.WriteLine("   ifmed -s - | ffmpeg -i - nope.mp4");
			Console.Error.WriteLine("   ifmed -s -h 192.168.1.2 - | ffmpeg -i - nope.mp4");
			Console.Error.WriteLine("   ifmed -s -h 192.168.1.2 -p 4001 - | ffmpeg -i - nope.mp4");
			Console.Error.WriteLine();
			Console.Error.WriteLine("   Server mode, receive stream with specific time and duration");
			Console.Error.WriteLine("   ifmed -s - | ffmpeg -i - -ss 120 -t 200 nope.mp4");
			Console.Error.WriteLine();
			Console.Error.WriteLine("Note:");
			Console.Error.WriteLine("1. Server needs to be run prior to client.");
			Console.Error.WriteLine("2. Pipe over WAN is not recommended unless");
			Console.Error.WriteLine("   you have speeds exceeding 10mbps with VPN (encryption).");
			Console.Error.WriteLine("3. Sending RAW stream over network is not recommended unless you have 10Gbps.\n");

			switch (code)
			{
				case 0:
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Error.WriteLine("Error! No command detected!, Type ifmed --help for more info.\n");
					Console.ResetColor();
					break;
				case 1:
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Error.WriteLine("Error! Invalid IP Address!, Type ifmed --help for more info.\n");
					Console.ResetColor();
					break;
				case 2:
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Error.WriteLine("Error! Wrong port number! Type ifmed --help for more info.\n");
					Console.ResetColor();
					break;
				case 3:
					Console.ForegroundColor = ConsoleColor.Red;
					Console.Error.WriteLine("Error! No pipe detected! Type ifmed --help for more info.\n");
					Console.ResetColor();
					break;
				default:
					break;
			}

			Environment.Exit(1);
		}
	}
}
