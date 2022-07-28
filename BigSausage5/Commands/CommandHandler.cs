using Discord.Commands;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BigSausage.Commands;
using System.Reflection;

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

		public async Task SetupAsync() {
			Logging.Log("Setting up CommandHandler...", Discord.LogSeverity.Info);
			_client.MessageReceived += HandleCommandsAsync;

			_commands.AddTypeReader(typeof(bool), new BooleanTypeReader());

			await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
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
