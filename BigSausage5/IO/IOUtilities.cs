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
using BigSausage.Commands;
using Discord.WebSocket;

namespace BigSausage.IO {
	internal class IOUtilities {

		private static readonly string[] LOCALE_NAMES = { "en_US", "funny_pirate" };
		public static bool AssertFileExists(string path, string filename) {
			if (path != null) {
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
					using (var stream = new FileStream(safeHandle, FileAccess.ReadWrite)) {
						stream.Write(bytes, 0, bytes.Length);
						stream.Close();
					}
					safeHandle.Close();
					safeHandle.Dispose();
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
						writer.Flush();
						writer.Close();
					}
					return true;
				}
			}
			return false;
		}

		public static Dictionary<string, Dictionary<string, string>> LoadAllLocales(string localeDirectory) {
			Logging.Log("Attempting to load localization files from \"" + localeDirectory + "\"...", LogSeverity.Verbose);
			Dictionary<string, Dictionary<string, string>> locales = new();
			foreach (string s in LOCALE_NAMES) {
				if (AssertDirectoryExists(localeDirectory)) {
					if (File.Exists(localeDirectory + "\\" + s + ".locale")) {
						Logging.Log("Loading localization file \"" + s + ".locale\"", LogSeverity.Verbose);
						string[] lines = File.ReadAllLines(localeDirectory + "\\" + s + ".locale");
						Dictionary<string, string> dict = new();
						foreach (string line in lines) {
							string s1 = line[..line.IndexOf("=")];
							string s2 = line[(line.IndexOf("=") + 1)..];
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

		private const int MaxRetries = 3;
		private const int DelayOnRetry = 1000;

		public async static void SavePermissionsToDisk(SerializableDictionary<ulong, SerializableDictionary<ulong, int>> permissions) {
			string path = Utils.GetProcessPathDir() + "\\Files\\";
			if (IO.IOUtilities.AssertDirectoryExists(path)) {
				Logging.Log("Writing data to Permissions.xml...", LogSeverity.Verbose);
				XmlSerializer serializer = new(typeof(SerializableDictionary<ulong, SerializableDictionary<ulong, int>>));
				for (int i = 1; i <= MaxRetries; i++) {
					try {
						Logging.Log("Creating writer...", LogSeverity.Debug);
						TextWriter writer = new StreamWriter(path + "Permissions.xml");
						Logging.Log("Serializing data...", LogSeverity.Debug);
						serializer.Serialize(writer, permissions);
						Logging.Log("Serialized data!", LogSeverity.Debug);
						writer.Close();
						break;
					} catch (Exception ex) when (i >= MaxRetries) {
						Logging.LogException(ex, "Exception saving permissions!");
						await Task.Delay(DelayOnRetry);
					}
				}
			}
		}

		public static SerializableDictionary<ulong, SerializableDictionary<ulong, int>> GetPermissionLevelsFromDisk() {
			if (IO.IOUtilities.AssertDirectoryExists(Utils.GetProcessPathDir() + "\\Files\\")) {
				if (IO.IOUtilities.AssertFileExists(Utils.GetProcessPathDir() + "\\Files\\", "Permissions.xml")) {
					Logging.Log("Creating serializer...", LogSeverity.Debug);
					XmlSerializer serializer = new(typeof(SerializableDictionary<ulong, SerializableDictionary<ulong, int>>));
					Logging.Log("Opening handle...", LogSeverity.Debug);
					SafeFileHandle handle = File.OpenHandle(Utils.GetProcessPathDir() + "\\Files\\Permissions.xml");
					using TextReader reader = new StreamReader(Utils.GetProcessPathDir() + "\\Files\\Permissions.xml");
					object? data = null;
					try {
						Logging.Log("Deserializing data...", LogSeverity.Debug);
						data = serializer.Deserialize(reader);
					} catch (Exception ex) {
						data = null;
						Logging.LogException(ex, "Error loading xml permissions!");
					}

					reader.Close();
					reader.Dispose();
					handle.Close();
					handle.Dispose();
					if (data != null) {
						if (data is SerializableDictionary<ulong, SerializableDictionary<ulong, int>> dictionary) {
							Logging.Log("Data loaded successfully!", LogSeverity.Debug);
							return dictionary;
						} else {
							Logging.Log("Loaded permissions but the data is of the wrong type! Defaulting...", LogSeverity.Error);
							Logging.LogErrorToFile(null, null, "Permissions file contained bad data.");

							return new SerializableDictionary<ulong, SerializableDictionary<ulong, int>>();
						}
					} else {
						Logging.Log("Permissions.xml cannot be loaded! Defaulting...", LogSeverity.Error);
						Logging.LogErrorToFile(null, null, "Permissions file loaded a null object.");

						return new SerializableDictionary<ulong, SerializableDictionary<ulong, int>>();
					}
				} else {
					Logging.Log("Permissions file doesn't exist!", LogSeverity.Warning);
					Logging.Log("Creating new permissions file...", LogSeverity.Warning);
					SerializableDictionary<ulong, SerializableDictionary<ulong, int>> perms = new();
					SavePermissionsToDisk(perms);
					Logging.Log("Done!", LogSeverity.Warning);
					return perms;
				}
			} else {
				Logging.Log("Somehow the files directory doesn't exist while trying to load permissions. Creating it now...", LogSeverity.Warning);
				SerializableDictionary<ulong, SerializableDictionary<ulong, int>> perms = new();
				SavePermissionsToDisk(perms);
				Logging.Log("Done!", LogSeverity.Warning);
				return perms;
			}
		}

		public static void SaveLinkablesToDisk(DiscordSocketClient? client, SerializableDictionary<ulong, List<Linkable>> linkables) {
			Logging.Log("Writing linkables to disk...", LogSeverity.Info);
			if (client != null) {
				foreach (IGuild guild in client.Guilds) {
					Logging.Log("Processing guild \"" + guild.Name + "\" (" + guild.Id.ToString() + ")...", LogSeverity.Info);
					string dir = Utils.GetProcessPathDir() + "\\Files\\Guilds\\" + guild.Id + "\\Linkables\\";
					string[] files = Directory.GetFiles(dir);
					XmlSerializer serializer = new(typeof(Linkable));
					if (linkables != null && linkables.ContainsKey(guild.Id)) {
						Logging.Log($"Beginning serialization of {linkables[guild.Id].Count} linkables...", LogSeverity.Debug);
						foreach (Linkable linkable in linkables[guild.Id]) {
							if (linkable.Filename != null) {
								AssertFileExists(dir, $"{linkable.Name}.xml");
								Logging.Log("Serializing " + linkable.Name + ".xml...", LogSeverity.Debug);
								TextWriter writer = new StreamWriter(dir + linkable.Name + ".xml");
								serializer.Serialize(writer, linkable);
								writer.Close();
							} else {
								Logging.Log("Filename for linkable is null! Skipping...", LogSeverity.Debug);
								continue;
							}
						}
					} else {
						Logging.Log($"Couldn't serialize files for guild as the data doesn't exist!", LogSeverity.Critical);
						continue;
					}
				}
			} else {
				Logging.Log("Client is null!", LogSeverity.Critical);
			}
		}


		public static SerializableDictionary<ulong, List<Linkable>> LoadLinkablesFromDisk(DiscordSocketClient? client) {
			Logging.Log("Loading linkables from disk...", LogSeverity.Debug);
			SerializableDictionary<ulong, List<Linkable>> ret = new();
			if (client != null) {
				foreach (IGuild guild in client.Guilds) {
					Logging.Log($"Loading linkables for guild \"{guild.Name}\" ({guild.Id})...", LogSeverity.Debug);
					string dir = Utils.GetProcessPathDir() + "\\Files\\Guilds\\" + guild.Id + "\\Linkables\\";
					AssertDirectoryExists(dir);
					string[] files = Directory.GetFiles(dir);
					XmlSerializer serializer = new(typeof(Linkable));
					List<Linkable> links = new();
					if (files != null && files.Length > 0) {
						foreach (string file in files) {
							if (File.Exists(file)) {
								Logging.Log($"Reading data from \"{file}\"...", LogSeverity.Debug);
								if (File.Exists(file) && file.EndsWith(".xml")) {
									Logging.Log("File is XML, deserializing...", LogSeverity.Debug);
									TextReader reader = new StreamReader(file);
									object? data = null;
									try {
										data = serializer.Deserialize(reader);
									} catch (Exception ex) {
										Logging.LogException(ex, "Tried to load Linkable and got bad data! " + file);
									}
									Logging.Log("Successfully deserialized linkable file!", LogSeverity.Debug);
									if (data != null) {
										if (data is Linkable linkable) {
											links.Add(linkable);
										} else {
											Logging.Log("Successfully deserialized linkable file but the loaded object is not a Linkable! \"" + file + "\"", LogSeverity.Error);
										}
									} else {
										Logging.Log("Failed to load Linkable from file: " + file, LogSeverity.Error);
									}
									reader.Close();
								}
							}
						}
					}
					Logging.Log($"Adding {links.Count} linkables to guild \"{guild.Name}\" ({guild.Id})...", LogSeverity.Debug);
					ret.Add(guild.Id, links);
				}
				return ret;
			} else {
				Logging.Log("Client is null!", LogSeverity.Critical);
				return ret;
			}
		}

		public static void DownloadFile(string URL, string path) {
			Logging.Log("Downloading to \"" + path + "\" from " + URL, LogSeverity.Verbose);
			HttpClient client = new();
			byte[] bytes = client.GetByteArrayAsync(URL).Result;
			File.WriteAllBytes(path, bytes);
			client.Dispose();
			Logging.Log("Done!", LogSeverity.Verbose);
		}

		public static void AddTtsToGuild(IGuild guild, string tts) {
			if (guild != null) {
				Logging.Log($"Attempting to add TTS \"{tts}\" to guild \"{guild.Name}\" ({guild.Id})", LogSeverity.Debug);
			}
		}
	}
}
