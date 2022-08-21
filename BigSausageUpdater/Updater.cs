using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace BigSausageUpdater {
	public class Updater {

		private readonly static string VERSION_URL = "https://s3.us-west-2.amazonaws.com/cdn.mizobogames.net/bigsausage/newVersion.txt";
		private readonly static string DOWNLOAD_URL = "https://s3.us-west-2.amazonaws.com/cdn.mizobogames.net/bigsausage/Versions/";
		private static string? botLocation;

		public Updater() {
		}

		public static Task Main(string[] args) => new Updater().MainAsync(args);


		public async Task MainAsync(string[] args) {
			try {
				string version = "";
				string callerID = "";
				if (args.Length == 0) {
					Console.WriteLine("[Updater] No version number supplied, getting newest version...");
					version = GetNewestVersion();
				} else if (args.Length != 4 && args.Length != 2) {
					string argstring = "";
					foreach (string arg in args) {
						argstring += arg + " ";
					}
					throw new ArgumentException("Invalid number of arguments (" + args.Length + "). \"" + argstring + "\"");
				} else if (args.Length == 2) {
					if (args[0] == "-version") {
						version = args[1];
					} else if (args[0] == "-callerID") {
						callerID = args[1];
					} else {
						throw new ArgumentException("Invalid argument at index 0!");
					}
				} else if (args.Length == 4) {
					if (args[0] == "-version") {
						version = args[1];
					} else if (args[0] == "-callerID") {
						callerID = args[2];
					} else {
						throw new ArgumentException("Invalid argument at index 0!");
					}
					if (args[2] == "-version") {
						version = args[3];
					} else if (args[2] == "-callerID") {
						callerID = args[3];
					} else {
						throw new ArgumentException("Invalid argument at index 2!");
					}
				}
				if(version == "") version = GetNewestVersion();
				string[] vs = version.Split('.');

				if (int.Parse(vs[0]) < 5) {
					throw new ArgumentException("The supplied version is too low! This program is only compatible with version 5.0.0 and above!");
				}
				Update(version, callerID);
			} catch (Exception ex) {
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine("[Updater] An exception occured while trying to update!");
				Console.WriteLine("[Updater] " + ex.GetType().FullName);
				Console.WriteLine("[Updater] " + ex.Message);
				Console.WriteLine("[Updater] " + ex.StackTrace);
				Console.ForegroundColor= ConsoleColor.White;
			}
		}

		private void Update(string version, string? callerID) {
			if (callerID != null) {
				Process bot = Process.GetProcessById(int.Parse(callerID));
				if (bot != null) {
					if (bot.MainModule != null) botLocation = bot.MainModule.FileName;
					Console.ForegroundColor = ConsoleColor.Yellow;
					Directory.CreateDirectory(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BigSausage");
					File.WriteAllText(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BigSausage\\ReadyToUpdate.bs", "yep!", Encoding.UTF8);
					Console.WriteLine("[Updater] Waiting for the bot to exit...");
					Console.ForegroundColor = ConsoleColor.White;
					bot.WaitForExit();
					File.Delete(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BigSausage\\ReadyToUpdate.bs");
				}
			}
			Console.WriteLine("[Updater] Performing update to version " + version + "...");
			Console.WriteLine("[Updater] ");
			Console.WriteLine("[Updater] Press any key to restart the bot...");
			Console.ReadKey();
			if (botLocation != null) {
				Process newBot = new Process();
				newBot.StartInfo.FileName = botLocation;
				newBot.Start();
				return;
			} else {
				Console.ForegroundColor = ConsoleColor.DarkRed;
				Console.WriteLine("[Updater] Bot location is null! Can't find the bot executable :(");
				Console.ForegroundColor = ConsoleColor.White;

				Console.WriteLine("[Updater] ");
				Console.WriteLine("[Updater] Press any key to exit...");
				Console.ReadKey();
			}
		}

		private string GetNewestVersion() {
			Console.WriteLine("[Updater] Downloading newVersion.txt...");
			return DownloadTextFile(VERSION_URL, GetProcessPathDir() + "\\new_version.txt")[0];
		}

		private static void DownloadFile(string URL, string localPath) {
			Console.WriteLine("[Updater] Downloading \"" + localPath + "\" from " + URL);
			HttpClient client = new HttpClient();
			byte[] bytes = client.GetByteArrayAsync(URL).Result;
			File.WriteAllBytes(localPath, bytes);
			client.Dispose();
			Console.WriteLine("Done!");
		}

		private string[] DownloadTextFile(string URL, string localPath) {
			DownloadFile(URL, localPath);

			string[] lines = File.ReadAllLines(localPath);
			return lines;
		}

		private static string GetProcessPathDir() {
			string dir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\BigSausageUpdater";
			if (Environment.ProcessPath != null) {
				dir = Environment.ProcessPath.Replace("BigSausageUpdater.exe", "");
			}
			return dir;
		}

	}
}
