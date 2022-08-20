using BigSausage.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigSausage.IO {
	internal class Linkables {

		private static SerializableDictionary<ulong, List<Linkable>>? _linkables;
		private static bool _initialized = false;

		public static void Initialize() {
			if (!_initialized) {
				Logging.Log("Initializing Linkables...", Discord.LogSeverity.Info);
				_linkables = IO.IOUtilities.LoadLinkablesFromDisk(BigSausage.GetClient());


			} else {
				Logging.Log("Linkable initialization was requested but linkables have already been initialized! ignoring...", Discord.LogSeverity.Warning);
			}
			_initialized = true;
		}

		public static void AddLinkableToGuild(IGuild guild, Linkable linkable) {
			if (!_initialized) {
				Logging.Log("Adding a Linkable was requested but Linkables have not yet been initialized!", LogSeverity.Warning);
				Initialize();
			}
			if (_linkables != null) {
				_linkables[guild.Id].Add(linkable);
				IO.IOUtilities.SaveLinkablesToDisk(BigSausage.GetClient(), _linkables);
			} else {
				Logging.Log("Linkables have been initialized but are still null!", LogSeverity.Critical);
			}
		}

		public static void Save() {
			if (_initialized && _linkables != null) {
				Logging.Log("Saving linkables...", LogSeverity.Debug);
				IO.IOUtilities.SaveLinkablesToDisk(BigSausage.GetClient(), _linkables);
			} else {
				Logging.Log("Linkables were never properly initialized, so they will not be saved.", LogSeverity.Warning);
			}
		}

		public static List<Linkable> GetLinkablesForGuild(IGuild guild) {
			if (!_initialized) {
				Logging.Log($"Linkables for guild \"{guild.Name}\"({guild.Id}) requested but linkables have not been initialized!", LogSeverity.Warning);
				Initialize();
			}
			if (_linkables == null) {
				Logging.Log("Linkables initialization failed!", LogSeverity.Critical);
				throw new IOException();
			}
			List<Linkable> linkables = new();

			if (_linkables.ContainsKey(guild.Id)) {
				linkables = _linkables[guild.Id];
			} else {
				Logging.Log($"Linkables is missing an entry for guild \"{guild.Name}\"({guild.Id})!", LogSeverity.Error);
			}

			return linkables;
		}

		public static List<Linkable> ScanForLinkableTriggers(IGuild guild, string message) {
			if (!_initialized) {
				Logging.Log("Trigger parsing was requested but Linkables have not yet been initialized!", LogSeverity.Warning);
				Initialize();
			}
			if(_linkables != null) {
				List<Linkable> result = new();
				string[] split = message.Split(' ');
				foreach (string word in split) {
					foreach (Linkable linkable in _linkables[guild.Id]) {
						if (linkable.Triggers == null) {
							Logging.Log("Triggers for linkable are null! " + linkable.Name, LogSeverity.Error);
						} else {
							foreach (string trigger in linkable.Triggers) {
								if (trigger == word) {
									result.Add(linkable);
									break;
								}
							}
						}
					}
				}
				return result;
			} else {
				Logging.Log("Linkables were initialized but are still null!", LogSeverity.Critical);
				return new();
			}
		}
	}
}
