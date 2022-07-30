using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigSausage.Commands {
	public class SlashCommandManager {

		private SlashCommandManager() { }


		public static string HandleSlashCommand(SocketSlashCommand command) {
			switch (command.Data.Name) {
				case "bs-tts":
					return TTSCommand(command);
				case "bs-upload":
					return UploadCommand(command);
				case "bs-help":
					return HelpCommand(command);
				default:
					return "Unrecognized command!";
			}
		}

		private static string TTSCommand(SocketSlashCommand command) {
			return "bs-tts";
		}

		private static string UploadCommand(SocketSlashCommand command) {
			return "bs-upload";
		}

		private static string HelpCommand(SocketSlashCommand command) {
			return "bs-help";
		}
	}
}
