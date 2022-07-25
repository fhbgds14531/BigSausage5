using System;
using Discord;
using Discord.Commands;

namespace BigSausage.Commands {

	public class BooleanTypeReader : TypeReader {
		public override Task<TypeReaderResult> ReadAsync(ICommandContext context, string input, IServiceProvider services) {
			bool result;
			if (bool.TryParse(input, out result)) return Task.FromResult(TypeReaderResult.FromSuccess(result));

			Logging.Log("[TypeReader Error] \"" + input + "\" could not be parsed as a boolean value!", LogSeverity.Error);
			return Task.FromResult(TypeReaderResult.FromError(CommandError.ParseFailed, "Input could not be parsed as a boolean"));
		}
	}
}
