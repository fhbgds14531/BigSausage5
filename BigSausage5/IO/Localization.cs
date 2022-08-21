using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace BigSausage.Localization {
	public class Localization {

		private readonly DiscordSocketClient _client;
		private Dictionary<string, Dictionary<string, string>> _localizationTables;
		private Dictionary<IGuild, string> _localizationSelections;

		private bool _initialized = false;

		public Localization(DiscordSocketClient client) {
			_client = client;
			_localizationSelections = new();
			_localizationTables = new();
			Initialize();
 		}

		private void Initialize() {
			_localizationTables = LoadLocalizationTables();
			this._initialized = true;
		}

		private Dictionary<string, Dictionary<string, string>> LoadLocalizationTables() {
			Dictionary<IGuild, string> selections = new();
			foreach (IGuild guild in _client.Guilds) {
				string? localeName;
				try {
					localeName = File.ReadLines(Utils.GetProcessPathDir() + "\\Files\\Guilds\\" + guild.Id + "\\selected_locale.bs").First();
				} catch (Exception e) {
					Logging.LogException(e, "loading localization for guild " + guild.Name + " (" + guild.Id + ")");
					localeName = "en_US";
				}
				selections.Add(guild, localeName);
			}
			_localizationSelections = selections;
			return IO.IOUtilities.LoadAllLocales(Utils.GetProcessPathDir() + "\\Files\\Locales");
		}

		public string GetLocalizedString(IGuild guild, string str) {
			try {
				if (!this._initialized) Initialize();
				string locale = _localizationSelections[guild];
				if (locale == null) {
					Logging.Warning(guild.Name + " (" + guild.Id + ") has no localization selection! Defaulting to en_US...");
					locale = "en_US";
					_localizationSelections[guild] = locale;
				}
				Dictionary<string, string>? localLocaleLUT = _localizationTables[locale];
				if (localLocaleLUT != null) {
					return localLocaleLUT[str];
				} else {
					Logging.LogErrorToFile(guild, null, "Failed to get localization!");
					Logging.Warning("Failed to get Localization for Guild " + guild.Name + "(" + guild.Id + ")!");
					return str;
				}
			} catch (Exception e) {
				Logging.LogException(e, "attempting to localize a string using " + _localizationSelections[guild]);
				Logging.Critical("The dictionary for " + _localizationSelections[guild] + " contains the following keys:");
				foreach(string s in _localizationTables[_localizationSelections[guild]].Keys) {
					Logging.Critical(s);
				}
				return str;
			}
		}

		public string GetLocalizedString(string locale, string str) {
			try {
				if (!this._initialized) Initialize();
				_localizationTables.TryGetValue(locale, out Dictionary<string, string>? localLocaleLUT);
				if (localLocaleLUT != null) {
					return localLocaleLUT[str];
				} else {
					Logging.LogErrorToFile(null, null, "Failed to get localization!");
					Logging.Error("Failed to get Localization \"" + locale + "\"!");
					return str;
				}
			} catch (Exception e) {
				Logging.LogException(e, "Exception loading localized string!");
				Logging.Warning("Defaulting to en_US");
				_localizationTables.TryGetValue("en_US", out Dictionary<string, string>? localLocaleLUT);
				if (localLocaleLUT != null) {
					return localLocaleLUT[str];
				} else {
					Logging.LogErrorToFile(null, null, "Failed to get even the default localization!");
					Logging.Critical("Failed to get even the default Localization! Something has gone seriously wrong.");
					return str;
				}
			}
		}

	}

	static class DefaultLocalizationStringsEN_US {

		public static List<string> GetDefaultStrings() {
			List<string> list = new();

			list.Add("command_help_general=Displays the help text for a command");
			list.Add("command_ping_description=Tests whether the bot has a connection");
			list.Add("command_TTS_description=Plays a random tts from the uploaded strings");

			return list;
		}

	}

}
