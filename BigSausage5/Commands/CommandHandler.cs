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

		private readonly string prefix = "!bs ";
		private readonly DiscordSocketClient _client;
		private readonly CommandService _commands;

		public CommandHandler(DiscordSocketClient client, CommandService commands) {
			Logging.Log("Initializing CommandHandler...", Discord.LogSeverity.Info);
			_client = client;
			_commands = commands;
		}

		public async Task InitGlobalSlashCommands(DiscordSocketClient client) {
			Logging.Log("Initializing global slash commands...", LogSeverity.Verbose);
			var globalUploadCommand = new SlashCommandBuilder();
			var globalHelpCommand = new SlashCommandBuilder();
			var globalTTSCommand = new SlashCommandBuilder();
			var l10n = BigSausage.GetLocalizationManager(client);

			Logging.Log("Initializing bs-upload...", LogSeverity.Debug);
			globalUploadCommand.WithName("bs-upload");
			globalUploadCommand.WithDescription(l10n.GetLocalizedString("en_US", "command_upload_description"));
			globalUploadCommand.WithDefaultMemberPermissions(GuildPermission.MentionEveryone);

			Logging.Log("Initializing bs-help...", LogSeverity.Debug);
			globalHelpCommand.WithName("bs-help");
			globalHelpCommand.WithDescription(l10n.GetLocalizedString("en_US", "command_help_general"));
			globalHelpCommand.WithDefaultMemberPermissions(GuildPermission.SendMessages);

			Logging.Log("Initializing bs-tts...", LogSeverity.Debug);
			globalTTSCommand.WithName("bs-tts");
			globalTTSCommand.WithDescription(l10n.GetLocalizedString("en_US", "command_TTS_description"));
			globalTTSCommand.WithDefaultMemberPermissions(GuildPermission.SendTTSMessages);

			try {
				Logging.Log("Injecting commands...", LogSeverity.Debug);
				await client.CreateGlobalApplicationCommandAsync(globalUploadCommand.Build());
				await client.CreateGlobalApplicationCommandAsync(globalHelpCommand.Build());
			}catch (HttpException ex) {
				Logging.LogException(ex, "Error processing slash command!");

			}
			return;
		}

		public async Task SetupAsync() {
			Logging.Log("Setting up CommandHandler...", Discord.LogSeverity.Info);
			_client.MessageReceived += HandleCommandsAsync;

			_commands.AddTypeReader(typeof(bool), new BooleanTypeReader());

			await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
		}

		public async Task HandleSlashCommandsAsync(SocketSlashCommand command) {
			await command.RespondAsync(SlashCommandManager.HandleSlashCommand(command));
		}

		public async Task HandleCommandsAsync(SocketMessage messageParam) {
			var message = messageParam as SocketUserMessage;
			if (message == null) return;
			int argPos = 0;

			if ((!message.HasStringPrefix(prefix, ref argPos, StringComparison.OrdinalIgnoreCase) || message.Author.IsBot)) return;

			Logging.Log("Recieved a command! \"" + message.Content + "\"", Discord.LogSeverity.Debug);
			Logging.Log("==============================================================================================", Discord.LogSeverity.Debug);
			Logging.Log("There are currently " + _commands.Modules.Count() + " command modules.", Discord.LogSeverity.Debug);
			foreach (var module in _commands.Modules) {
				Logging.Log("\tModule " + module.Name + ":", Discord.LogSeverity.Debug);
				foreach (var command in module.Commands) {
					Logging.Log("\t\t" + command.Name, Discord.LogSeverity.Debug);
				}
			}
			Logging.Log("==============================================================================================", Discord.LogSeverity.Debug);

			var context = new SocketCommandContext(_client, message);
			await _commands.ExecuteAsync(context, argPos, null, MultiMatchHandling.Best);
			Logging.Log("Executed command!", Discord.LogSeverity.Verbose);
		}
	}
}
