using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;

namespace BigSausage.Commands.CommandTypes {
	public class ControlModule : ModuleBase<SocketCommandContext>{

		[Command("sd")]
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
			Process externalProcess = new();
			externalProcess.StartInfo.FileName = Utils.GetProcessPathDir() + "\\Files\\Updater\\BigSausageUpdater.exe";
			externalProcess.StartInfo.Arguments = args;
			externalProcess.Start();
			Logging.Log("Waiting for updater to initialize before exiting...", Discord.LogSeverity.Info);
			double seconds = 0.0;
			while (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BigSausage\\ReadyToUpdate.bs") && seconds < 60) {
				await Task.Delay(500);
				Logging.Log("Still waiting...", Discord.LogSeverity.Verbose);
				seconds += 0.5;
			}
			if (seconds >= 60) {
				Logging.Log("Updater timed out. Killing process...", Discord.LogSeverity.Error);
				externalProcess.Kill();
				Logging.Log("Killed Updater process, returning to normal functionality...", Discord.LogSeverity.Error);
				await Utils.ReplyToMessageFromCommand(Context, "Sorry, the updater process took too long and was aborted.");
				return;
			} else {
				await BigSausage.TimeToClose();
			}
		}

	}
}
