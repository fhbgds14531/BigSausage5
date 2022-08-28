using Discord.Commands;

namespace BigSausage.Commands.CommandTypes {
    public class DebugModule : ModuleBase<SocketCommandContext> {

        [Command("debug", RunMode = RunMode.Async)]
        [RequireOwner]
        public async Task DebugCommand(string command) {
            switch (command) {
                case "load-perms":
                    await Utils.ReplyToMessageFromCommand(Context, "Debug command accepted!");
                    Logging.Debug("Loading permissions...");
                    Permissions.Permissions.Reload();
                    await Utils.ReplyToMessageFromCommand(Context, $"Permissions loaded! You are {Permissions.Permissions.GetUserPermissionLevelInGuild(Context.Guild, Context.User)}");
                    break;
                case "save-perms":
                    await Utils.ReplyToMessageFromCommand(Context, "Debug command accepted!");
                    Permissions.Permissions.Save();
                    await Utils.ReplyToMessageFromCommand(Context, "Permissions saved!");
                    break;
                case "fmm-test":
                    var longString = Utils.FormattingTestString + Utils.FormattingTestString + Utils.FormattingTestString + Utils.FormattingTestString;
                    Utils.SplitMessageFormattingAware($"{longString} {longString} {longString}").ForEach(async s => await Utils.ReplyToMessageFromCommand(Context, s));
                    break;
                default:
                    await Utils.ReplyToMessageFromCommand(Context, $"No such debug command type ({command}) exists!");
                    break;
            }
            return;
        }

    }
}
