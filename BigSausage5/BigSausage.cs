﻿using BigSausage.IO;
using BigSausage.Localization;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Diagnostics;

namespace BigSausage {

    public class BigSausage {

        private static readonly string TOKEN = "BigSausageDEBUG.token";
        private static Process _process = null!;
        private static DiscordSocketClient _client = null!;
        private static MessageHandler _commandHandler = null!;
        private static AudioManager _audioManager = null!;
        private static LocalizationManager? _localizationManager;
        private static TaskCompletionSource<bool> _shutdownTask = null!;
        private static DateTime _startTime;

        public BigSausage() {
            _shutdownTask = new TaskCompletionSource<bool>();
            _startTime = DateTime.Now;
            _process = Process.GetCurrentProcess();
            _audioManager = new AudioManager();
            Logging.Info($"Launching BigSausage V ({typeof(BigSausage).Assembly.GetName().Version})");
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

            _commandHandler = new MessageHandler(_client, commandService);

            _client.SlashCommandExecuted += _commandHandler.HandleSlashCommandsAsync;

        }

        public static Task Main() => new BigSausage().MainAsync();


        public static AudioManager GetAudioManager() {
            return _audioManager;
        }

        public static bool ShouldRun() {
            if (_shutdownTask != null) {
                return _shutdownTask.Task.Result;
            } else {
                return true;
            }
        }

        public async Task MainAsync() {
            _process = Process.GetCurrentProcess();
            if (_client != null) {

                await _client.LoginAsync(TokenType.Bot, File.ReadAllText(Utils.GetProcessPathDir() + "\\Files\\Tokens\\" + TOKEN));
                if (_commandHandler != null) await _commandHandler.SetupAsync();
                await _client.StartAsync();

                if (_shutdownTask != null) await _shutdownTask.Task;
                await Shutdown();
            }

        }

        public static TimeSpan GetUptime() {
            return DateTime.Now.Subtract(_startTime);
        }

        public static Process GetBotMainProcess() {
            return _process;
        }

        public static Task TimeToClose() {
            if (_shutdownTask != null) _shutdownTask.SetResult(true);
            return Task.CompletedTask;
        }

        private static async Task Shutdown(int code = 0) {
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
            var start = DateTime.Now;
            await _commandHandler.InitGlobalSlashCommands(_client);

            foreach (IGuild guild in _client.Guilds) {
                ulong guildId = guild.Id;
                string guildName = guild.Name;
                Logging.Verbose("========================================================");
                Logging.Info("Setting up guild " + guildId + " (" + guildName + ")...");
                string guildFilePath = Utils.GetProcessPathDir() + "\\Files\\Guilds\\" + guildId;

                Logging.Verbose("========================================================");
                Logging.Verbose("Asserting guild directory...");
                IO.IOUtilities.AssertDirectoryExists(guildFilePath);

                Logging.Verbose("========================================================");
                Logging.Verbose("Asserting guild name file...");
                IO.IOUtilities.AssertFileExists(guildFilePath, "." + guildName);

                Logging.Verbose("========================================================");
                Logging.Verbose("Asserting guild image directory...");
                IO.IOUtilities.AssertDirectoryExists(guildFilePath + "\\Linkables\\Images");

                Logging.Verbose("========================================================");
                Logging.Verbose("Asserting guild audio directory...");
                IO.IOUtilities.AssertDirectoryExists(guildFilePath + "\\Linkables\\Audio");

                Logging.Verbose("========================================================");
                Logging.Verbose("Asserting guild TTS file...");
                IO.IOUtilities.AssertFileExists(guildFilePath, "TTS.txt");

                Logging.Verbose("========================================================");
                Logging.Verbose("Asserting guild localization selection file...");
                IO.IOUtilities.AssertFileExists(guildFilePath, "selected_locale.bs");
                string[] contents = File.ReadAllLines(guildFilePath + "\\selected_locale.bs");
                if (contents.Length == 0) {
                    Logging.Warning("Localization file for " + guildName + " (" + guildId + ") is empty! Defaulting to en_US");
                    contents = new string[] { "en_US" };
                    File.WriteAllText(guildFilePath + "\\selected_locale.bs", contents[0]);
                }
                Logging.Verbose("========================================================");


            }
            Logging.Verbose("========================================================");
            Logging.Verbose("Asserting permissions initialization...");
            Permissions.Permissions.Initialize();
            Logging.Verbose("========================================================");
            Logging.Verbose("Asserting Linkables initialization...");
            IO.Linkables.Initialize();
            Logging.Verbose("========================================================");

            var end = DateTime.Now;
            var diff = end.Subtract(start);
            Logging.Info($"Initialization sequence complete in {diff.TotalSeconds} seconds!");

            foreach (string s in Utils.GetASCIILogo()) {
                Logging.Info(s);
            }
            return;
        }

        public static LocalizationManager GetLocalizationManager(DiscordSocketClient client) {
            if (_localizationManager == null) {
                Logging.Info("Initializing LocalizationManager...");
                long start = DateTime.Now.Ticks;
                _localizationManager = new LocalizationManager(client);
                long end = DateTime.Now.Ticks;
                long duration = end - start;
                duration /= System.TimeSpan.TicksPerSecond;
                Logging.Warning("Finished initializing localization in " + duration.ToString("F3") + " seconds!");
            }
            return _localizationManager;
        }

        public static DiscordSocketClient GetClient() {
            return _client;
        }

    }

}