using BigSausage.Commands;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Diagnostics;

namespace BigSausage {

	public class BigSausage {

		private static readonly string TOKEN = "BigSausageDEBUG.token";
		private static Process? _process;
		private static DiscordSocketClient? _client;
		private static CommandHandler? _commandHandler;
		private static Localization? _localizationManager;
		private static TaskCompletionSource<bool>? _shutdownTask;

		public BigSausage() {
			_process = Process.GetCurrentProcess();
			DiscordSocketConfig discordSocketConfig = new() {
				GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.GuildMembers
			};


			_client = new DiscordSocketClient(discordSocketConfig);
			_client.Log += Logging.Log;
			_client.Ready += BotReady;


			CommandServiceConfig config = new();
			config.SeparatorChar = ' ';
			config.CaseSensitiveCommands = false;
			config.IgnoreExtraArgs = true;

			CommandService commandService = new(config);

			_commandHandler = new CommandHandler(_client, commandService);

			_client.SlashCommandExecuted += _commandHandler.HandleSlashCommandsAsync;

			_shutdownTask = new TaskCompletionSource<bool>();
		}

		public static Task Main() => new BigSausage().MainAsync();


		public async Task MainAsync() {
			_process = Process.GetCurrentProcess();
			if (_client != null) {
				
				await _client.LoginAsync(TokenType.Bot, File.ReadAllText(Utils.GetProcessPathDir() + "\\Files\\Tokens\\" + TOKEN));
				if(_commandHandler != null) await _commandHandler.SetupAsync();
				await _client.StartAsync();

				if (_shutdownTask != null) await _shutdownTask.Task;
				await Shutdown();
			}

		}

		public static Process GetBotMainProcess() {
			return _process ?? Process.GetCurrentProcess();
		}

		public static Task TimeToClose() {
			if(_shutdownTask != null) _shutdownTask.SetResult(true);
			return Task.CompletedTask;
		}

		private async static Task Shutdown(int code = 0) {
			Logging.Info("Beginning shutdown sequence...");
			IO.Linkables.Save();
			if (_client != null) {
				await _client.LogoutAsync();
				Logging.Info("Logged out of discord.");
			} else {
				Logging.Verbose("Client is null so no need to log out.");
			}
			Logging.Info("Shutting Down...");
			Permissions.Permissions.Save();
			if (_client != null) {
				await _client.StopAsync();
				Logging.Info("Successfully stopped client!");
			} else {
				Logging.Info("Client is null so no need to stop.");
			}
			Logging.Info("Successfully shut down!");
			Environment.Exit(code);
		}

		private async Task BotReady() {
			if(_commandHandler != null) await _commandHandler.InitGlobalSlashCommands(_client);
			if(_client == null) return; 

			foreach (IGuild guild in _client.Guilds) {
				ulong guildId = guild.Id;
				string guildName = guild.Name;
				Logging.Log("Setting up guild " + guildId + " (" + guildName + ")...", LogSeverity.Info);
				string guildFilePath = Utils.GetProcessPathDir() + "\\Files\\Guilds\\" + guildId;

				Logging.Log("Asserting guild directory...", LogSeverity.Verbose);
				IO.IOUtilities.AssertDirectoryExists(guildFilePath);

				Logging.Log("Asserting guild name file...", LogSeverity.Verbose);
				IO.IOUtilities.AssertFileExists(guildFilePath, "." + guildName);

				Logging.Log("Asserting guild image directory...", LogSeverity.Verbose);
				IO.IOUtilities.AssertDirectoryExists(guildFilePath + "\\Linkables\\Images");

				Logging.Log("Asserting guild audio directory...", LogSeverity.Verbose);
				IO.IOUtilities.AssertDirectoryExists(guildFilePath + "\\Linkables\\Audio");

				Logging.Log("Asserting guild TTS file...", LogSeverity.Verbose);
				IO.IOUtilities.AssertFileExists(guildFilePath, "TTS.txt");
				
				Logging.Log("Asserting guild localization selection file...", LogSeverity.Verbose);
				IO.IOUtilities.AssertFileExists(guildFilePath, "selected_locale.bs");
				string[] contents = File.ReadAllLines(guildFilePath + "\\selected_locale.bs");
				if (contents.Length == 0) {
					Logging.Log("Localization file for " + guildName + " (" + guildId + ") is empty! Defaulting to en_US", LogSeverity.Warning);
					contents = new string[]{ "en_US" };
					File.WriteAllText(guildFilePath + "\\selected_locale.bs", contents[0]);
				}

				
			}
			Logging.Log("Asserting permissions initialization...", LogSeverity.Verbose);
			Permissions.Permissions.Initialize();
			Logging.Log("Asserting Linkables initialization...", LogSeverity.Verbose);
			IO.Linkables.Initialize();
			return;
		}

		public static Localization GetLocalizationManager(DiscordSocketClient client) {
			if (_localizationManager == null) {
				Logging.Log("LocalizationManager was requested but has not yet been initialized! Initializing it now...", LogSeverity.Warning);
				long start = DateTime.Now.Ticks;
				_localizationManager = new Localization(client);
				long end = DateTime.Now.Ticks;
				long duration = end - start;
				duration /= System.TimeSpan.TicksPerSecond;
				Logging.Log("Finished initializing localization in " + duration.ToString("F3") + " seconds!", LogSeverity.Warning);
			}
			return _localizationManager;
		}

		public static DiscordSocketClient GetClient() {
			if (_client != null) {
				return _client;
			} else {
				Logging.LogException(new InvalidOperationException("Attempted to access client when it was null!"), "Please wait for the bot to initialize before calling this method.");
				_ = Shutdown(-1);
				return new DiscordSocketClient();
			}
		}

	}

}