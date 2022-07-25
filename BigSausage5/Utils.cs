using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigSausage {
	public class Utils {

		public static string GetProcessPathDir() {
			if (Environment.ProcessPath != null) {
				return Environment.ProcessPath.Replace("\\BigSausage5.exe", "");
			} else {
				Logging.Log("Failed to get ProcessPath! Defaulting to Desktop.", Discord.LogSeverity.Error);
				Logging.LogErrorToFile(null, null, "Failed to get ProcessPath!");
				return Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\BigSausage fallback directory";
			}
		}

		public static async Task ReplyToMessageFromCommand(SocketCommandContext context, string reply) {
			Discord.MessageReference message = new Discord.MessageReference(context.Message.Id, context.Channel.Id, context.Guild.Id);
			await context.Channel.SendMessageAsync(reply, false, null, null, null, message, null, null, null);
		}

		public static Task FailedTask {
			get {
				throw null;
			}
		}

	}

	
}
