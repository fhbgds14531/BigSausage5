using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace BigSausage.Commands.CommandTypes {
	public class DebugModule : ModuleBase<SocketCommandContext> {

		[Command("debug")]
		[RequireOwner]
		public async Task DebugCommand(string command) {
			switch (command) {
				case "load-permissions":
					await (Utils.ReplyToMessageFromCommand(Context, "Debug command accepted!"));
					Logging.Log("Loading permissions...", Discord.LogSeverity.Debug);
					Permissions.Permissions.Reload();
					await Utils.ReplyToMessageFromCommand(Context, $"Permissions loaded! You are {Permissions.Permissions.GetUserPermissionLevelInGuild(Context.Guild, Context.User)}");
					break;
				case "save-permissions":
					await (Utils.ReplyToMessageFromCommand(Context, "Debug command accepted!"));
					Permissions.Permissions.Save();
					await Utils.ReplyToMessageFromCommand(Context, "Permissions saved!");
					break;
				default:
					await Utils.ReplyToMessageFromCommand(Context, $"No such debug command type ({command}) exists!");
					break;
			}
			return;
		}

	}
}
