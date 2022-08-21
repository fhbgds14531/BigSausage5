using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;

namespace BigSausage.Commands.CommandTypes {
	public class InfoModule : ModuleBase<SocketCommandContext> {

		[Command("ping")]
		public async Task PingAsync() {
			await Utils.ReplyToMessageFromCommand(Context, BigSausage.GetClient().ConnectionState.ToString());
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
