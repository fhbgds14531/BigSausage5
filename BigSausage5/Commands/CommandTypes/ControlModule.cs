using System;
using System.Collections.Generic;
using System.Diagnostics;
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
		public async Task Update([Remainder] string version = "") {
			Logging.Log("The bot will now attempt to perform a self-update...", Discord.LogSeverity.Info);
			string args = "";
			if (version != "") {
				args = "-version " + version;
			}
			args += " -callerID " + BigSausage.GetBotMainProcess().Id;
			await Utils.ReplyToMessageFromCommand(Context, "Updating...");
			Logging.Log("Launching updater process...", Discord.LogSeverity.Info);
			Process externalProcess = new Process();
			externalProcess.StartInfo.FileName = Utils.GetProcessPathDir() + "\\Files\\Updater\\BigSausageUpdater.exe";
			externalProcess.StartInfo.Arguments = args;
			externalProcess.Start();
			Logging.Log("Waiting for updater to initialize before exiting...", Discord.LogSeverity.Info);
			while (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BigSausage\\ReadyToUpdate.bs")) {
				await Task.Delay(500);
				Logging.Log("Still waiting...", Discord.LogSeverity.Verbose);
			}
			await BigSausage.TimeToClose();
		}

	}
}
