using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace BigSausageUpdater {
	public class Updater {

		private readonly static string VERSION_URL = "https://s3.us-west-2.amazonaws.com/cdn.mizobogames.net/bigsausage/newVersion.txt";
		private readonly static string DOWNLOAD_URL = "https://s3.us-west-2.amazonaws.com/cdn.mizobogames.net/bigsausage/Versions/";

		public Updater() { }

		public static Task Main(string[] args) => new Updater().MainAsync(args);


		public async Task MainAsync(string[] args) {
			string version = "5.0.0";
			if (args.Length == 0) {
				Console.WriteLine("No version number supplied, getting newest version...");
				version = GetNewestVersion();
			} else if (args.Length != 2) {
				throw new ArgumentException("Invalid number of arguments.");
			} else {
				if (args[0] == "-version") {
					version = args[1];
				} else {
					throw new ArgumentException("Invalid argument! The first argument MUST be \"-version\"");
				}
			}
			string[] vs = version.Split('.');

			if (int.Parse(vs[0]) < 5) {
				throw new ArgumentException("The supplied version is too low! This program is only compatible with version 5.0.0 and above!");
			}
			Update(version);
		}

		private void Update(string version) {
			Console.WriteLine("Performing update to version " + version + "...");
		}

		private string GetNewestVersion() {
			Console.WriteLine("Downloading newVersion.txt...");
			return DownloadTextFile(VERSION_URL, GetProcessPathDir() + "\\new_version.txt")[0];
		}

		private static void DownloadFile(string URL, string localPath) {
			Console.WriteLine("Downloading \"" + localPath + "\" from " + URL);
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
