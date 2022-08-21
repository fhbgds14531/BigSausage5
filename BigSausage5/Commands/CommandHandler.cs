using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BigSausage.Commands;
using System.Reflection;
using Discord;
using Discord.Net;

namespace BigSausage {
	public class CommandHandler {

		public static readonly string BOT_PREFIX = "!bs ";
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;
		private readonly List<SlashCommandBuilder> _slashCommandBuilders;

		public CommandHandler(DiscordSocketClient client, CommandService commands) {
			Logging.Info("Initializing CommandHandler...");
			_client = client;
			_commands = commands;
			_slashCommandBuilders = new List<SlashCommandBuilder>();
		}

		public async Task InitGlobalSlashCommands(DiscordSocketClient? client) {
			Logging.Verbose("Initializing global slash commands...");
			if (client != null) {
				var l10n = BigSausage.GetLocalizationManager(client);

				Logging.Debug("Initializing bs-ping...");
				_slashCommandBuilders.Add(new SlashCommandBuilder().WithName("bs-ping").WithDescription(l10n.GetLocalizedString("en_US", "command_ping_description")));

				Logging.Debug("Initializing bs-help...");
				_slashCommandBuilders.Add(new SlashCommandBuilder().WithName("bs-help").WithDescription(l10n.GetLocalizedString("en_US", "command_help_general"))
					.WithDefaultMemberPermissions(GuildPermission.SendMessages));

				Logging.Debug("Initializing bs-tts...");
				_slashCommandBuilders.Add(new SlashCommandBuilder().WithName("bs-tts").WithDescription(l10n.GetLocalizedString("en_US", "command_TTS_description"))
					.WithDefaultMemberPermissions(GuildPermission.SendTTSMessages));



				try {
					Logging.Debug("Injecting slash commands...");
					foreach (SlashCommandBuilder slashCommandBuilder in _slashCommandBuilders) {
						Logging.Debug($"Injecting {slashCommandBuilder.Name}...");
						await client.CreateGlobalApplicationCommandAsync(slashCommandBuilder.Build());
					}
				} catch (HttpException ex) {
					Logging.LogException(ex, "Error processing slash command!");
				}
				return;
			} else {
				Logging.Critical("Client is null! Cannot Initialize slash commands.");
				return;
			}
		}

		public async Task SetupAsync() {
			Logging.Info("Setting up CommandHandler...");
			_client.MessageReceived += HandleCommandsAsync;

			_commands.AddTypeReader(typeof(bool), new BooleanTypeReader());

			await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
		}

		public async Task HandleSlashCommandsAsync(SocketSlashCommand command) {
			await command.RespondAsync(SlashCommandManager.HandleSlashCommand(command));
		}

		public async Task HandleCommandsAsync(SocketMessage messageParam) {
			if (messageParam is not SocketUserMessage message) return;
			int argPos = 0;

			if ((!message.HasStringPrefix(BOT_PREFIX, ref argPos, StringComparison.OrdinalIgnoreCase) || message.Author.IsBot)) return;

			Logging.Debug("Recieved a command! \"" + message.Content + "\"");

			var context = new SocketCommandContext(_client, message);
			await _commands.ExecuteAsync(context, argPos, null, MultiMatchHandling.Exception);
			Logging.Debug("Command execution finished.");
		}
	}
}
