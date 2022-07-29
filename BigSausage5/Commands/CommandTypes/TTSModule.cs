using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace BigSausage.Commands.CommandTypes {
	public class TTSModule : ModuleBase<SocketCommandContext> {

		[Command("add-tts")]
		[Summary("Adds a string to the TTS file")]
		public async Task AddTTS([Remainder] string ttsString) {
			if (Permissions.Permissions.UserMeetsPermissionRequirements(Context.Guild, Context.User, Permissions.EnumPermissionLevel.High)) {
				await Utils.ReplyToMessageFromCommand(Context, ttsString);
			} else {
				await Utils.ReplyToMessageFromCommand(Context, "Sorry, You do not have permission to use that command! The minimum permission required is High. Your current permission level is " +
					Permissions.Permissions.GetUserPermissionLevelInGuild(Context.Guild, Context.User).ToString());
			}
		}

	}
}
