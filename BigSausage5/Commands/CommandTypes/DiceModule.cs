using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigSausage.Commands.CommandTypes {
	public class DiceModule : ModuleBase<SocketCommandContext> {

		public async Task Roll([Remainder] string dice) {
			await Utils.ReplyToMessageFromCommand(Context, "This command is not yet functional, sorry.");
		}

	}

	internal class Die {
		private int sides;

		public Die(int sides) {
			this.sides = sides;
		}

		public int Roll() {
			return new Random().Next(sides) + 1;
		}
	}
}
