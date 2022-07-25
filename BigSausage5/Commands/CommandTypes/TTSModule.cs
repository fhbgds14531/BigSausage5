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
			await Utils.ReplyToMessageFromCommand(Context, ttsString);
		}

	}
}
