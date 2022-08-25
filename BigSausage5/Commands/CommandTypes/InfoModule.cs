using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BigSausage.Commands.CommandTypes {
	public class InfoModule : ModuleBase<SocketCommandContext> {

		[Command("ping")]
		public async Task PingAsync() {
			await Utils.ReplyToMessageFromCommand(Context, BigSausage.GetClient().ConnectionState.ToString());
		}

		[Command("stats")]
		public async Task StatsAsync() {
			var start = DateTime.Now;
			Logging.Debug("Calculating statistics...");
			Logging.Debug("Calculating uptime...");
			TimeSpan uptime = BigSausage.GetUptime();
			Logging.Debug("Getting total guilds...");
			int totalGuilds = BigSausage.GetClient().Guilds.Count;
			long linkableDiskUsage_Global = 0L;
			long ttsDiskUsage_Global = 0L;
			long systemFileDiskUsage_Global = 0L;

			string filesDir = Utils.GetProcessPathDir() + "\\Files";


			Logging.Debug("Tallying system file sizes...");
			Logging.Debug("Main directory...");
			foreach (string file in Directory.GetFiles(Utils.GetProcessPathDir())) {
				systemFileDiskUsage_Global += new FileInfo(file).Length;
			}
			Logging.Debug("Permissions...");
			systemFileDiskUsage_Global += new FileInfo(filesDir + "\\Permissions.xml").Length;
			Logging.Debug("Locales...");
			foreach (string file in Directory.GetFiles(filesDir + "\\Locales\\")) {
				systemFileDiskUsage_Global += new FileInfo(file).Length;
			}
			Logging.Debug("Updater...");
			foreach (string file in Directory.GetFiles(filesDir + "\\Updater\\")) {
				systemFileDiskUsage_Global += new FileInfo(file).Length;
			}
			foreach (string file in Directory.GetFiles(filesDir + "\\Tokens\\")) {
				Logging.Debug("Tokens...");
				systemFileDiskUsage_Global += new FileInfo(file).Length;
			}
			Logging.Debug("Logging...");
			foreach (string file in Directory.GetFiles(filesDir + "\\Logging\\")) {
				systemFileDiskUsage_Global += new FileInfo(file).Length;
			}
			Logging.Debug("Error Logs...");
			foreach (string dir in Directory.GetDirectories(filesDir + "\\Logging\\")) {
				foreach(string file in Directory.GetFiles(dir)) {
					systemFileDiskUsage_Global += new FileInfo(file).Length;
				}
			}

			int ttsFileLength = 0;
			Logging.Debug("Calculating guild-based statistics...");
			Dictionary<ulong, Dictionary<string, long>> guildStats = new();
			List<int> ttsLengths = new();
			foreach (IGuild guild in BigSausage.GetClient().Guilds) {
				Logging.Debug($"{guild.Name} ({guild.Id})...");
				string guildPath = Utils.GetProcessPathDir() + $"\\Files\\Guilds\\{guild.Id}";
				Logging.Debug("Locale selection...");
				systemFileDiskUsage_Global += new FileInfo(guildPath + "\\selected_locale.bs").Length;
				Logging.Debug($"TTS file ({guildPath}\\TTS.txt)...");
				int i = File.ReadAllLinesAsync(guildPath + "\\TTS.txt").Result.Length;
				ttsLengths.Add(i);
				if (guild.Id == Context.Guild.Id) ttsFileLength = i;
				long ttsSize = new FileInfo(guildPath + "\\TTS.txt").Length;
				ttsDiskUsage_Global += ttsSize;
				guildStats[guild.Id] = new();
				guildStats[guild.Id].Add("tts", ttsSize);
				guildStats[guild.Id].Add("linkables", 0);
				guildStats[guild.Id].Add("audio", 0);
				guildStats[guild.Id].Add("image", 0);
				Logging.Debug("Linkables...");
				foreach(Linkable l in IO.Linkables.GetLinkablesForGuild(guild)) {
					if (l.type == EnumLinkableType.Audio) guildStats[guild.Id]["audio"]++;
					if (l.type == EnumLinkableType.Image) guildStats[guild.Id]["image"]++;
				}

				foreach (string dir in Directory.GetDirectories(guildPath + "\\Linkables\\")) {
					foreach (string file in Directory.GetFiles(dir)) {
						var size = new FileInfo(file).Length;
						linkableDiskUsage_Global += size;
						guildStats[guild.Id]["linkables"] += size;
					}
				}
				Logging.Debug("Linkable metadata...");
				foreach (string file in Directory.GetFiles(guildPath + "\\Linkables\\")) {
					var size = new FileInfo(file).Length;
					linkableDiskUsage_Global += size;
					guildStats[guild.Id]["linkables"] += size;
				}
			}

			int ttsLineAverage = 0;
			ttsLengths.ForEach(i => ttsLineAverage += i);
			ttsLineAverage /= ttsLengths.Count;

			Logging.Debug("Total disk usage...");
			long totalDiskUsage = ttsDiskUsage_Global + linkableDiskUsage_Global + systemFileDiskUsage_Global;
			Logging.Debug("Percentages...");
			double ttsPercentGlobal = Math.Round((double)(ttsDiskUsage_Global * 100.0 / totalDiskUsage + 0.0000001), 2);
			double linkablePercentGlobal = Math.Round((double)(linkableDiskUsage_Global * 100.0 / totalDiskUsage + 0.0000001), 2);
			double systemFilePercentGlobal = Math.Round((double)(systemFileDiskUsage_Global * 100.0 / totalDiskUsage + 0.0000001), 2);

			long totalGuildUsage = guildStats[Context.Guild.Id]["tts"] + guildStats[Context.Guild.Id]["linkables"];
			double guildUsagePercentGlobal = Math.Round((double)(totalGuildUsage * 100.0 / totalDiskUsage + 0.0000001), 2);
			double guildTTSPercentGlobalTotal = Math.Round((double)(guildStats[Context.Guild.Id]["tts"] * 100.0 / totalDiskUsage + 0.00001), 2);
			double guildTTSPercentGlobalTTS = Math.Round((double)((guildStats[Context.Guild.Id]["tts"] + 1) * 100.0 / ttsDiskUsage_Global + 0.00001), 2);
			double guildTTSPercentGuild = Math.Round((double)(guildStats[Context.Guild.Id]["tts"] * 100.0 / totalGuildUsage + 0.00001), 2);


			double guildLinkablePercentGlobalTotal = Math.Round((double)(guildStats[Context.Guild.Id]["linkables"] * 100.0 / totalDiskUsage + 0.0000001), 2);
			double guildLinkablePercentGlobalLinkables = Math.Round((double)(guildStats[Context.Guild.Id]["linkables"] * 100.0 / linkableDiskUsage_Global + 0.0000001), 2);
			double guildLinkablePercentGuild = Math.Round((double)(guildStats[Context.Guild.Id]["linkables"] * 100.0 / totalGuildUsage + 0.0000001), 2);
			int ttsOverstep = ttsFileLength - ttsLineAverage;

			long audioLinkablesGuild = guildStats[Context.Guild.Id]["audio"];
			long imageLinkablesGuild = guildStats[Context.Guild.Id]["image"];

			long audioLinkablesGlobalTotal = 0;
			long imageLinkablesGlobalTotal = 0;

			foreach(Dictionary<string, long> stat in guildStats.Values) {
				audioLinkablesGlobalTotal += stat["audio"];
				imageLinkablesGlobalTotal += stat["image"];
			}

			long globalLinkableTotal = audioLinkablesGlobalTotal + imageLinkablesGlobalTotal;

			long totalGuildLinkables = audioLinkablesGuild + imageLinkablesGuild;

			double guildAudioLinkablesPercent = Math.Round((double)(audioLinkablesGuild * 100.0 / totalGuildLinkables + 0.0000001));
			double guildImageLinkablesPercent = 100.0 - guildAudioLinkablesPercent;

			double guildImageLinkablesPercentGlobalImages = Math.Round((double)(imageLinkablesGuild * 100.0 / imageLinkablesGlobalTotal + 0.0000001));
			double guildImageLinkablesPercentGlobalLinkables = Math.Round((double)(imageLinkablesGuild * 100.0 / globalLinkableTotal + 0.0000001));

			double guildAudioLinkablesPercentGlobalAudio = Math.Round((double)((audioLinkablesGuild + 0.01) * 100.0 / audioLinkablesGlobalTotal + 0.0000001));
			double guildAudioLinkablesPercentGlobalLinkables = Math.Round((double)(audioLinkablesGuild * 100.0 / globalLinkableTotal + 0.0000001));

			if (ttsFileLength == 0) {
				guildTTSPercentGlobalTTS = 0;
				guildTTSPercentGuild = 0;
				guildTTSPercentGlobalTotal = 0;
			}

			if(audioLinkablesGuild == 0) {
				guildAudioLinkablesPercent = 0;
				guildAudioLinkablesPercentGlobalAudio = 0;
				guildAudioLinkablesPercentGlobalLinkables = 0;
			}

			if (imageLinkablesGuild == 0) {
				guildImageLinkablesPercent = 0;
				guildImageLinkablesPercentGlobalImages = 0;
				guildImageLinkablesPercentGlobalLinkables = 0;
			}

			Logging.Debug("Creating response...");
			List<string> response = new() {
				"**BigSausage Statistics:**",
				$"Uptime: `{uptime:dd\\:hh\\:mm\\:ss}`",
				$"Version: `{typeof(BigSausage).Assembly.GetName().Version}`",
				$"Total guilds: `{totalGuilds}`",
				$"Total global disk usage: `{Utils.BytesToHumanReadableString(totalDiskUsage)}`",
				$"  - Linkables:    `{Utils.BytesToHumanReadableString(linkableDiskUsage_Global)} ({linkablePercentGlobal}%)`",
				$"  - TTS files:    `{Utils.BytesToHumanReadableString(ttsDiskUsage_Global)} ({ttsPercentGlobal}%)`",
				$"  - System files: `{Utils.BytesToHumanReadableString(systemFileDiskUsage_Global)} ({systemFilePercentGlobal}%)`",
				"",
				$"*Guild stats for \"{Context.Guild.Name}\"*:",
				$"  Total guild disk usage: `{Utils.BytesToHumanReadableString(totalGuildUsage)} ({guildUsagePercentGlobal}% of global usage)`",
				$"    - Linkables: `{Utils.BytesToHumanReadableString(guildStats[Context.Guild.Id]["linkables"])}`",
				$"      - `{guildLinkablePercentGuild}%` of guild total disk usage",
				$"      - `{guildLinkablePercentGlobalTotal}%` of global total disk usage",
				$"      - `{guildLinkablePercentGlobalLinkables}%` of global linkables",
				$"      - `{guildLinkablePercentGlobalLinkables}%` of global linkables",
				$"      - `{audioLinkablesGuild} ({guildAudioLinkablesPercent}%)` audio clips",
				$"        - `{guildAudioLinkablesPercentGlobalAudio}%` of global audio clips",
				$"        - `{guildAudioLinkablesPercentGlobalLinkables}%` of global linkables",
				$"      - `{imageLinkablesGuild} ({guildImageLinkablesPercent}%)` images",
				$"        - `{guildImageLinkablesPercentGlobalImages}%` of global images",
				$"        - `{guildImageLinkablesPercentGlobalLinkables}%` of global linkables",
				$"    - TTS: `{Utils.BytesToHumanReadableString(guildStats[Context.Guild.Id]["tts"])}`",
				$"      - `{guildTTSPercentGuild}%` of guild total disk usage",
				$"      - `{guildTTSPercentGlobalTotal}%` of global total disk usage",
				$"      - `{guildTTSPercentGlobalTTS}%` of global TTS file disk usage",
				$"  TTS file length: `{ttsFileLength} line{(ttsFileLength == 1 ? "" : "s")}`",
				$"    - {(ttsOverstep == 0 ? "Exactly" : " `" + Math.Abs(ttsOverstep) + (ttsOverstep > 0 ? $"` above " : "` below "))} the average line count of `{ttsLineAverage}`"
			};



			string result = "";
			Logging.Debug("Packaging response...");
			response.ForEach(s => result += s + "\n");
			var end = DateTime.Now;

			var diff = end.Subtract(start);
			Logging.Debug($"Executed stats command in {diff.TotalSeconds} seconds.");
			await Utils.ReplyToMessageFromCommand(Context, result);
		}

		[Command("help")]
		[Summary("Gets help generally or for a specific command")]
		public async Task HelpAsync([Summary("The specific command you would like help with")] string commandName = "general") {
			Logging.Verbose("Executing help command...");
			Localization.Localization? localization = BigSausage.GetLocalizationManager(BigSausage.GetClient());
			if (localization == null) {
				await Utils.ReplyToMessageFromCommand(Context, "command_help_" + commandName);
			} else {
				Logging.Verbose("Sending help info!");
				string localized = localization.GetLocalizedString(this.Context.Guild, "command_help_" + commandName.ToLower());
				if(localized.Equals("command_help_" + commandName)) {
					localized = "Sorry, the command you are requesting help with (\"" + commandName + "\") doesn't have an entry in the language file.";
				}

				await Utils.ReplyToMessageFromCommand(Context, localized);
			}
		}

	}
}
