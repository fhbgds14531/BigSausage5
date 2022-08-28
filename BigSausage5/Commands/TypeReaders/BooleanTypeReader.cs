using Discord.Commands;

namespace BigSausage.Commands {

    public class BooleanTypeReader : TypeReader {
        public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
            if (bool.TryParse(input, out bool result)) return Task.FromResult(TypeReaderResult.FromSuccess(result));

            Logging.Error("[TypeReader Error] \"" + input + "\" could not be parsed as a boolean value!");
            return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as a boolean"));
        }
    }
}
