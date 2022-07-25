using Microsoft.Win32.SafeHandles;
using Discord;
using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using System.Data;
using System.Xml;

namespace BigSausage.IO {
	internal class IOUtilities {

		private static readonly string[] LOCALE_NAMES = { "en_US", "funny_pirate" };
		public static bool AssertFileExists(string path, string filename) {
			if(path != null) {
				SafeFileHandle handle = File.OpenHandle(path + "\\" + filename, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.ReadWrite, FileOptions.None);
				handle.Close();
				return true;
			}
			return false;
		}

		public static bool AssertDirectoryExists(string path) {
			if (path != null) {
				if (!Directory.Exists(path)) {
					Directory.CreateDirectory(path);
				}
				return true;
			}
			return false;
		}

		public static bool SaveToFile(string path, string filename, byte[] bytes) {
			if (AssertDirectoryExists(path)) {
				if (AssertFileExists(path, filename)) {
					SafeFileHandle safeHandle = File.OpenHandle(path + "\\" + filename, FileMode.Open, FileAccess.ReadWrite, FileShare.ReadWrite, FileOptions.None, 0);
					FileStream stream = new FileStream(safeHandle, FileAccess.ReadWrite);
					stream.Write(bytes, 0, bytes.Length);
					stream.Close();
					safeHandle.Close();
					return true;
				}
			}
			return false;
		}

		public static bool WriteLineToFile(string line, string path, string filename) {
			if (AssertDirectoryExists(path)) {
				if (AssertFileExists(path, filename)) {
					using (StreamWriter writer = File.AppendText(path + "\\" + filename)) {
						writer.WriteLine(line);
					}
					return true;
				}
			}
			return false;
		}

		public static Dictionary<string, Dictionary<string, string>> LoadAllLocales(string localeDirectory) {
			Logging.Log("Attempting to load localization files from \"" + localeDirectory + "\"...", LogSeverity.Verbose);
			Dictionary<string, Dictionary<string, string>> locales = new Dictionary<string, Dictionary<string,string>>();
			foreach(string s in LOCALE_NAMES){
				if (AssertDirectoryExists(localeDirectory)) {
					if (File.Exists(localeDirectory + "\\" + s + ".locale")) {
						Logging.Log("Loading localization file \"" + s + ".locale\"", LogSeverity.Verbose);
						string[] lines = File.ReadAllLines(localeDirectory + "\\" + s + ".locale");
						Dictionary<string, string> dict = new Dictionary<string, string>();
						foreach (string line in lines) {
							string s1 = line.Substring(0, line.IndexOf("="));
							string s2 = line.Substring(line.IndexOf("=") + 1);
							Logging.Log("Loaded pair: " + s1 + ", " + s2, LogSeverity.Verbose);
							dict.Add(s1, s2);
						}
						Logging.Log("Loaded " + dict.Count + " lines to localization manager from locale \"" + s + "\"", LogSeverity.Info);
						locales.Add(s, dict);
					}
				}
			}
			Logging.Log("Success!", LogSeverity.Verbose);
			return locales;
		}

		public static void SavePermissionsToDisk(Dictionary<ulong, Dictionary<ulong, int>> permissions) {
			if (IO.IOUtilities.AssertDirectoryExists(Utils.GetProcessPathDir() + "\\Files\\")) {
				Logging.Log("Writing data to Permissions.xml...", LogSeverity.Verbose);
				XmlSerializer serializer = new XmlSerializer(typeof(Dictionary<ulong, Dictionary<ulong, int>>));
				TextWriter writer = new StreamWriter(Utils.GetProcessPathDir() + "\\Files\\Permissions.xml");
				serializer.Serialize(writer, permissions);
				writer.Close();
			}
		}

		public static Dictionary<ulong, Dictionary<ulong, int>> GetPermissionLevelsFromDisk() {
			if (IO.IOUtilities.AssertDirectoryExists(Utils.GetProcessPathDir() + "\\Files\\")) {
				if (IO.IOUtilities.AssertFileExists(Utils.GetProcessPathDir() + "\\Files\\", "Permissions.xml")) {
					XmlSerializer serializer = new XmlSerializer(typeof(Dictionary<ulong, Dictionary<ulong, int>>));
					TextReader reader = new StreamReader(Utils.GetProcessPathDir() + "\\Files\\Permissions.xml");
					object? data = serializer.Deserialize(reader);
					if(data != null) {
						if (data is Dictionary<ulong, Dictionary<ulong, int>>) {
							return (Dictionary<ulong, Dictionary<ulong, int>>)data;
						} else {
							Logging.Log("Loaded permissions but the data is of the wrong type! Defaulting...", LogSeverity.Error);
							Logging.LogErrorToFile(null, null, "Permissions file contained bad data.");

							return new Dictionary<ulong, Dictionary<ulong, int>>();
						}
					} else {
						Logging.Log("Permissions.xml cannot be loaded! Defaulting...", LogSeverity.Error);
						Logging.LogErrorToFile(null, null, "Permissions file loaded a null object.");

						return new Dictionary<ulong, Dictionary<ulong, int>>();
					}
				} else {
					Logging.Log("Permissions file doesn't exist!", LogSeverity.Warning);
					Logging.Log("Creating new permissions file...", LogSeverity.Warning);
					Dictionary<ulong, Dictionary<ulong, int>> perms = new Dictionary<ulong, Dictionary<ulong, int>>();
					SavePermissionsToDisk(perms);
					Logging.Log("Done!", LogSeverity.Warning);
					return perms;
				}
			} else {
				Logging.Log("Somehow the files directory doesn't exist while trying to load permissions. Creating it now...", LogSeverity.Warning);
				Dictionary<ulong, Dictionary<ulong, int>> perms = new Dictionary<ulong, Dictionary<ulong, int>>();
				SavePermissionsToDisk(perms);
				Logging.Log("Done!", LogSeverity.Warning);
				return perms;
			}
		}

		public static void AddTtsToGuild(IGuild guild, string tts) {
			if(guild != null) {

			}
		}
	}
}
