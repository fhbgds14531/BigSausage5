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
			Logging.Verbose("Attempting to load localization files from \"" + localeDirectory + "\"...");
			Dictionary<string, Dictionary<string, string>> locales = new();
			foreach (string s in LOCALE_NAMES) {
				if (AssertDirectoryExists(localeDirectory)) {
					if (File.Exists(localeDirectory + "\\" + s + ".locale")) {
						Logging.Verbose("Loading localization file \"" + s + ".locale\"");
						string[] lines = File.ReadAllLines(localeDirectory + "\\" + s + ".locale");
						Dictionary<string, string> dict = new();
						foreach (string line in lines) {
							string s1 = line[..line.IndexOf("=")];
							string s2 = line[(line.IndexOf("=") + 1)..];
							Logging.Verbose("Loaded pair: " + s1 + ", " + s2);
							dict.Add(s1, s2);
						}
						Logging.Info($"Loaded {dict.Count} lines into localization manager from locale \"{s}\"");
						locales.Add(s, dict);
					}else if(s == "en_US") {
						Logging.Warning("en_US does not exist! Writing default strings...");
						List<string> lines = Localization.DefaultLocalizationStringsEN_US.GetDefaultStrings();
						File.WriteAllLines(localeDirectory + "\\" + s + ".locale", lines);
						Logging.Warning($"Wrote {lines.Count} lines to file!");
						Dictionary<string, string> dict = new();
						foreach (string line in lines) {
							string s1 = line[..line.IndexOf("=")];
							string s2 = line[(line.IndexOf("=") + 1)..];
							Logging.Verbose("Loaded pair: " + s1 + ", " + s2);
							dict.Add(s1, s2);
						}
						Logging.Warning($"Loaded {dict.Count} lines into localization manager from default en_US strings.");
						locales.Add(s, dict);
					}
				}
			}
			Logging.Verbose("Success!");
			return locales;
		}

		private const int MaxRetries = 3;
		private const int DelayOnRetry = 1000;

		public async static void SavePermissionsToDisk(SerializableDictionary<ulong, SerializableDictionary<ulong, int>> permissions) {
			string path = Utils.GetProcessPathDir() + "\\Files\\";
			if (IO.IOUtilities.AssertDirectoryExists(path)) {
				Logging.Verbose("Writing data to Permissions.xml...");
				XmlSerializer serializer = new(typeof(SerializableDictionary<ulong, SerializableDictionary<ulong, int>>));
				for (int i = 1; i <= MaxRetries; i++) {
					try {
						Logging.Debug("Creating writer...");
						TextWriter writer = new StreamWriter(path + "Permissions.xml");
						Logging.Debug("Serializing data...");
						serializer.Serialize(writer, permissions);
						Logging.Debug("Serialized data!");
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
					Logging.Debug("Creating serializer...");
					XmlSerializer serializer = new(typeof(SerializableDictionary<ulong, SerializableDictionary<ulong, int>>));
					Logging.Debug("Opening handle...");
					SafeFileHandle handle = File.OpenHandle(Utils.GetProcessPathDir() + "\\Files\\Permissions.xml");
					using TextReader reader = new StreamReader(Utils.GetProcessPathDir() + "\\Files\\Permissions.xml");
					object? data = null;
					try {
						Logging.Debug("Deserializing data...");
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
							Logging.Debug("Data loaded successfully!");
							return dictionary;
						} else {
							Logging.Error("Loaded permissions but the data is of the wrong type! Defaulting...");
							Logging.LogErrorToFile(null, null, "Permissions file contained bad data.");

							return new SerializableDictionary<ulong, SerializableDictionary<ulong, int>>();
						}
					} else {
						Logging.Error("Permissions.xml cannot be loaded! Defaulting...");
						Logging.LogErrorToFile(null, null, "Permissions file loaded a null object.");

						return new SerializableDictionary<ulong, SerializableDictionary<ulong, int>>();
					}
				} else {
					Logging.Warning("Permissions file doesn't exist!");
					Logging.Warning("Creating new permissions file...");
					SerializableDictionary<ulong, SerializableDictionary<ulong, int>> perms = new();
					SavePermissionsToDisk(perms);
					Logging.Warning("Done!");
					return perms;
				}
			} else {
				Logging.Warning("Somehow the files directory doesn't exist while trying to load permissions. Creating it now...");
				SerializableDictionary<ulong, SerializableDictionary<ulong, int>> perms = new();
				SavePermissionsToDisk(perms);
				Logging.Warning("Done!");
				return perms;
			}
		}

		public static void SaveLinkablesToDisk(DiscordSocketClient? client, SerializableDictionary<ulong, List<Linkable>> linkables) {
			Logging.Info("Writing linkables to disk...");
			if (client != null) {
				foreach (IGuild guild in client.Guilds) {
					Logging.Info("Processing guild \"" + guild.Name + "\" (" + guild.Id.ToString() + ")...");
					string dir = Utils.GetProcessPathDir() + "\\Files\\Guilds\\" + guild.Id + "\\Linkables\\";
					string[] files = Directory.GetFiles(dir);
					XmlSerializer serializer = new(typeof(Linkable));
					if (linkables != null && linkables.ContainsKey(guild.Id)) {
						Logging.Debug($"Beginning serialization of {linkables[guild.Id].Count} linkables...");
						foreach (Linkable linkable in linkables[guild.Id]) {
							if (linkable.Filename != null) {
								AssertFileExists(dir, $"{linkable.Name}.xml");
								Logging.Debug("Serializing " + linkable.Name + ".xml...");
								TextWriter writer = new StreamWriter(dir + linkable.Name + ".xml");
								serializer.Serialize(writer, linkable);
								writer.Close();
							} else {
								Logging.Debug("Filename for linkable is null! Skipping...");
								continue;
							}
						}
					} else {
						Logging.Critical($"Couldn't serialize files for guild as the data doesn't exist!");
						continue;
					}
				}
			} else {
				Logging.Critical("Client is null!");
			}
		}


		public static SerializableDictionary<ulong, List<Linkable>> LoadLinkablesFromDisk(DiscordSocketClient? client) {
			Logging.Debug("Loading linkables from disk...");
			SerializableDictionary<ulong, List<Linkable>> ret = new();
			if (client != null) {
				foreach (IGuild guild in client.Guilds) {
					Logging.Debug($"Loading linkables for guild \"{guild.Name}\" ({guild.Id})...");
					string dir = Utils.GetProcessPathDir() + "\\Files\\Guilds\\" + guild.Id + "\\Linkables\\";
					AssertDirectoryExists(dir);
					string[] files = Directory.GetFiles(dir);
					XmlSerializer serializer = new(typeof(Linkable));
					List<Linkable> links = new();
					if (files != null && files.Length > 0) {
						foreach (string file in files) {
							if (File.Exists(file)) {
								Logging.Debug($"Reading data from \"{file}\"...");
								if (File.Exists(file) && file.EndsWith(".xml")) {
									Logging.Debug("File is XML, deserializing...");
									TextReader reader = new StreamReader(file);
									object? data = null;
									try {
										data = serializer.Deserialize(reader);
									} catch (Exception ex) {
										Logging.LogException(ex, "Tried to load Linkable and got bad data! " + file);
									}
									Logging.Debug("Successfully deserialized linkable file!");
									if (data != null) {
										if (data is Linkable linkable) {
											links.Add(linkable);
										} else {
											Logging.Error("Successfully deserialized linkable file but the loaded object is not a Linkable! \"" + file + "\"");
										}
									} else {
										Logging.Error("Failed to load Linkable from file: " + file);
									}
									reader.Close();
								}
							}
						}
					}
					Logging.Debug($"Adding {links.Count} linkables to guild \"{guild.Name}\" ({guild.Id})...");
					ret.Add(guild.Id, links);
				}
				return ret;
			} else {
				Logging.Critical("Client is null!");
				return ret;
			}
		}

		public static void DownloadFile(string URL, string path) {
			Logging.Verbose("Downloading to \"" + path + "\" from " + URL);
			HttpClient client = new();
			byte[] bytes = client.GetByteArrayAsync(URL).Result;
			File.WriteAllBytes(path, bytes);
			client.Dispose();
			Logging.Verbose("Done!");
		}

		public static void AddTtsToGuild(IGuild guild, string tts) {
			if (guild != null) {
				Logging.Debug($"Attempting to add TTS \"{tts}\" to guild \"{guild.Name}\" ({guild.Id})");
			}
		}
	}
}
