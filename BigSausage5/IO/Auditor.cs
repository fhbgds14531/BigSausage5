using Discord;
using Discord.Commands;

namespace BigSausage.IO {

    public class Auditor {

        private static readonly string AUDIT_LOG_PATH = @"Files\Guilds\%G\Audit.log";


        public static void LogEvent(IGuild guild, string text) {
            IO.IOUtilities.AppendLineToFile(text, Utils.GetProcessPathDir() + "\\" + AUDIT_LOG_PATH.Replace("%G", $"{guild.Id}"));
            Logging.Info($"Event Logged! \"{text}\"");
        }

        public static void LogCommandExecuted(SocketCommandContext context) {
            LogEvent(context.Guild, $"Command Executed! Guild: {context.Guild.Name} ({context.Guild.Id}), "
                                    + $"Author: {context.Message.Author.Username} ({context.Message.Author.Id}), "
                                    + $"Message content: \"{context.Message.Content}\"");
        }

        public static void LogCommandFinished(SocketCommandContext context, IResult result) {
            LogEvent(context.Guild, $"Command Executed! Guild: {context.Guild.Name} ({context.Guild.Id}), "
                                    + $"Author: {context.Message.Author.Username} ({context.Message.Author.Id}), "
                                    + $"Message content: \"{context.Message.Content}\", Result: {result}");
        }
    }
}
