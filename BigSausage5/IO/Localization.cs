using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;

namespace BigSausage {
	public class Localization {

		private readonly DiscordSocketClient _client;
		private Dictionary<string, Dictionary<string, string>> _localizationTables;
		private Dictionary<IGuild, string> _localizationSelections;

		private bool _initialized = false;

		public Localization(DiscordSocketClient client) {
			_client = client;
			Initialize();
 		}

		private void Initialize() {
			_localizationTables = LoadLocalizationTables();
			this._initialized = true;
		}

		private Dictionary<string, Dictionary<string, string>> LoadLocalizationTables() {
			Dictionary<IGuild, string> selections = new Dictionary<IGuild, string>();
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
					Logging.Log(guild.Name + " (" + guild.Id + ") has no localization selection! Defaulting to en_US...", LogSeverity.Warning);
					locale = "en_US";
					_localizationSelections[guild] = locale;
				}
				Dictionary<string, string>? localLocaleLUT = _localizationTables[locale];
				if (localLocaleLUT != null) {
					return localLocaleLUT[str];
				} else {
					Logging.LogErrorToFile(guild, null, "Failed to get localization!");
					Logging.Log("Failed to get Localization for Guild " + guild.Name + "(" + guild.Id + ")!", LogSeverity.Error);
					return str;
				}
			} catch (Exception e) {
				Logging.LogException(e, "attempting to localize a string using " + _localizationSelections[guild]);
				Logging.Log("The dictionary for " + _localizationSelections[guild] + " contains the following keys:", LogSeverity.Critical);
				foreach(string s in _localizationTables[_localizationSelections[guild]].Keys) {
					Logging.Log(s, LogSeverity.Critical);
				}
				return str;
			}
		}

		public string GetLocalizedString(string locale, string str) {
			if (!this._initialized) Initialize();
			Dictionary<string, string>? localLocaleLUT;
			_localizationTables.TryGetValue(locale, out localLocaleLUT);
			if (localLocaleLUT != null) {
				return localLocaleLUT[str];
			} else {
				Logging.LogErrorToFile(null, null, "Failed to get localization!");
				Logging.Log("Failed to get Localization \"" + locale + "\"!", LogSeverity.Error);
				return str;
			}
		}

	}
}
