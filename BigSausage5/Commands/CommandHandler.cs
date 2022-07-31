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
		private List<SlashCommandBuilder> _slashCommandBuilders;

		public CommandHandler(DiscordSocketClient client, CommandService commands) {
			Logging.Log("Initializing CommandHandler...", Discord.LogSeverity.Info);
			_client = client;
			_commands = commands;
			_slashCommandBuilders = new List<SlashCommandBuilder>();
		}

		public async Task InitGlobalSlashCommands(DiscordSocketClient client) {
			Logging.Log("Initializing global slash commands...", LogSeverity.Verbose);
			var l10n = BigSausage.GetLocalizationManager(client);

			Logging.Log("Initializing bs-upload...", LogSeverity.Debug);
			_slashCommandBuilders.Add(new SlashCommandBuilder().WithName("bs-upload")
				.WithDescription(l10n.GetLocalizedString("en_US", "command_upload_description")).WithDefaultMemberPermissions(GuildPermission.MentionEveryone));

			Logging.Log("Initializing bs-help...", LogSeverity.Debug);
			_slashCommandBuilders.Add(new SlashCommandBuilder().WithName("bs-help").WithDescription(l10n.GetLocalizedString("en_US", "command_help_general"))
				.WithDefaultMemberPermissions(GuildPermission.SendMessages));

			Logging.Log("Initializing bs-tts...", LogSeverity.Debug);
			_slashCommandBuilders.Add(new SlashCommandBuilder().WithName("bs-tts").WithDescription(l10n.GetLocalizedString("en_US", "command_TTS_description"))
				.WithDefaultMemberPermissions(GuildPermission.SendTTSMessages));



			try {
				Logging.Log("Injecting commands...", LogSeverity.Debug);
				foreach (SlashCommandBuilder slashCommandBuilder in _slashCommandBuilders) {
					Logging.Log($"Injecting {slashCommandBuilder.Name}...", LogSeverity.Debug);
					await client.CreateGlobalApplicationCommandAsync(slashCommandBuilder.Build());
				}
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

			var context = new SocketCommandContext(_client, message);
			await _commands.ExecuteAsync(context, argPos, null, MultiMatchHandling.Best);
			Logging.Log("Executed command!", Discord.LogSeverity.Debug);
		}
	}
}
