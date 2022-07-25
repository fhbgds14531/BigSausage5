using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace BigSausage.Commands.CommandTypes {
	public class ControlModule : ModuleBase<SocketCommandContext>{

		[Command("shutdown")]
		[Summary("Shuts down the bot")]
		[RequireOwner]
		public async Task Shutdown() {
			await Utils.ReplyToMessageFromCommand(Context, "Shutting down...");
			await BigSausage.TimeToClose();
		}

		[Command("update")]
		[RequireOwner]
		[Summary("Updates the bot to the latest version.")]
		public Task Update() {

			return Task.CompletedTask;
		}

	}
}
