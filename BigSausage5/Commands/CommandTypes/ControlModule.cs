using Discord;
using Discord.Commands;
using System.Diagnostics;

namespace BigSausage.Commands.CommandTypes {
    public class ControlModule : ModuleBase<SocketCommandContext> {

        [Command("clear", RunMode = RunMode.Async)]
        public async Task Clear(int range = 30) {
            if (range < 1) range = 1;
            if (range > 100) range = 100;
            List<IMessage> messagesToDelete = new();

            Logging.Verbose($"Clearing the last {range} message{(range == 1 ? "" : "s")} in guild {Context.Guild.Name} ({Context.Guild.Id})");

            IEnumerable<IMessage> grabbed = await AsyncEnumerableExtensions.FlattenAsync(Context.Channel.GetMessagesAsync(range));
            int tooOldCount = 0;
            foreach (IMessage message in grabbed) {
                if (message != null) {
                    if (message.Content.StartsWith(MessageHandler.BOT_PREFIX) || message.Author.Id == BigSausage.GetClient().CurrentUser.Id) {
                        if (message.Timestamp > DateTime.UtcNow.Subtract(TimeSpan.FromDays(14))) {
                            messagesToDelete.Add(message);
                        } else {
                            tooOldCount++;
                        }
                    }
                }
            }
            if (tooOldCount > 0) Logging.Verbose($"{tooOldCount} message{(tooOldCount == 1 ? " was" : "s were")} too old to bulk delete!");
            Logging.Verbose($"Found {messagesToDelete.Count} message{(messagesToDelete.Count == 1 ? "" : "s")} to delete!");
            Logging.Debug("Deleting messages...");
            await ((ITextChannel)Context.Channel).DeleteMessagesAsync(messagesToDelete);
            Logging.Verbose($"Deleted {messagesToDelete.Count} message{(messagesToDelete.Count == 1 ? "" : "s")}!");
        }

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
            Logging.Info("The bot will now attempt to perform a self-update...");
            string args = "";
            if (version != "") {
                args = "-version " + version;
            }
            args += " -callerID " + BigSausage.GetBotMainProcess().Id;
            await Utils.ReplyToMessageFromCommand(Context, "Updating...");
            Logging.Info("Launching updater process...");
            Process externalProcess = new();
            externalProcess.StartInfo.FileName = Utils.GetProcessPathDir() + "\\Files\\Updater\\BigSausageUpdater.exe";
            externalProcess.StartInfo.Arguments = args;
            externalProcess.Start();
            Logging.Info("Waiting for updater to initialize before exiting...");
            double seconds = 0.0;
            while (!File.Exists(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + "\\BigSausage\\ReadyToUpdate.bs") && seconds < 60) {
                await Task.Delay(500);
                Logging.Verbose("Still waiting...");
                seconds += 0.5;
            }
            if (seconds >= 60) {
                Logging.Error("Updater timed out. Killing process...");
                externalProcess.Kill();
                Logging.Error("Killed Updater process, returning to normal functionality...");
                await Utils.ReplyToMessageFromCommand(Context, "Sorry, the updater process took too long and was aborted.");
                return;
            } else {
                await BigSausage.TimeToClose();
            }
        }

    }
}
