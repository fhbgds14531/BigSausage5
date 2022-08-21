using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BigSausage.Commands.CommandTypes {
	public class DiceModule : ModuleBase<SocketCommandContext> {

		[Command("roll")]
		public async Task Roll(string dice) {
			Logging.Debug("Executing dice roll...");
			await Utils.ReplyToMessageFromCommand(Context, Process(dice));
		}

		private string Process(string input) {
			Logging.Debug("Processing input...");
			DiceRoll? diceRoll = GetRoll(input);
			if (diceRoll != null) {
				Logging.Debug($"Parsed the input \"{input}\" and recieved a non-null value!");
				diceRoll.Roll();
				Logging.Debug($"Rolled the dice! The numerical result is {diceRoll.GetIntTotal()} and the string result is as follows:");
				string result = $"Result: {diceRoll.GetIntTotal()}\n`";
				List<string> results = diceRoll.GetStringResult();
				for (int i = 0; i < results.Count; i++) {
					Logging.Debug($"Roll result {i}: {results[i]}");
					result += results[i] + (i == results.Count - 1 ? "" : ",");
				}
				result += "`";
				return result;
			}
			Logging.LogErrorToFile(Context.Guild, Context.Message, $"Error parsing dice roll... Input was \"{input}\"");
			return "Error parsing dice roll!";
		}

		private DiceRoll? GetRoll(string currentRoll) {
			try {
				Logging.Debug($"Evaluating {currentRoll} for rolls...");
				Regex roll = new("([+|-]*)(\\d+)d(\\d+)|([+|-])(\\d+)");
				if (roll.IsMatch(currentRoll)) {
					string newString = "";
					Match m = roll.Match(currentRoll); int sign = 1;
					if (m.Groups[1] != null) if (m.Groups[1].Value == "-") sign = -1;
					if (m.Groups[5].Value != null && m.Groups[5].Value != string.Empty) {
						if (m.Groups[4] != null) if (m.Groups[4].Value == "-") sign = -1;
						Logging.Debug($"Group 5 is not null ({m.Groups[5]})! Treating this input as an int!");
						Logging.Debug($"Parsed capture ({m.Groups[0].Value}) and found [Number = {m.Groups[5].Value}, Sign = {sign}]");
						newString = m.Captures[0].Length == currentRoll.Length ? "done" : currentRoll[m.Captures[0].Length..];
						return new DiceRoll(int.Parse(m.Groups[5].Value), GetRoll(newString), sign);
					}
					int dice = int.Parse(m.Groups[2].Value);
					int sides = int.Parse(m.Groups[3].Value);
					Logging.Debug($"Parsed capture ({m.Groups[0].Value}) and found [Dice = {dice}, Sides = {sides}, Sign = {sign}]");
					newString = m.Captures[0].Length == currentRoll.Length ? "done" : currentRoll[m.Captures[0].Length..];
					Logging.Debug($"Finished parsing group! New string is \"{newString}\"");
					return new DiceRoll(dice, sides, GetRoll(newString), sign);
				} else {
					Logging.Debug($"Reached the end of the tree! returning null as the input string is \"{currentRoll}\"");
					return null;
				}
			} catch (Exception ex) {
				Logging.LogException(ex, "Failed to GetRoll");
				return null;
			}
		}

		internal class DiceRoll {
			readonly int signMult;
			readonly int sides;
			readonly int dice;
			readonly DiceRoll? mod;

			int total;

			private List<string> rolls;

			public DiceRoll(int dice, int sides, DiceRoll? modifier, int sign) {
				this.signMult = sign;
				this.sides = sides;
				this.dice = dice;
				this.mod = modifier;
				this.rolls = new List<string>();
				Logging.Debug($"Created new DiceRoll: Dice={dice} Sides={sides} Sign={signMult}");
			}

			public DiceRoll(int number) {
				sides = 1;
				dice = number;
				this.mod = null;
				this.signMult = 1;
				this.rolls = new List<string>();
				Logging.Debug($"Created new DiceRoll: Dice={dice} Sides={sides} Sign={signMult}");
			}

			public DiceRoll(int number, DiceRoll? mod, int sign) {
				sides = -1;
				dice = number;
				this.mod = mod;
				this.signMult = sign;
				this.rolls = new List<string>();
				Logging.Debug($"Created new DiceRoll: Dice={dice} Sides={sides} Sign={signMult}");
			}

			public DiceRoll(int sides, int numberOfDice, DiceRoll? modifier) {
				this.sides = sides;
				this.dice = numberOfDice;
				this.mod = modifier;
				this.signMult = 1;
				this.rolls = new List<string>();
				Logging.Debug($"Created new DiceRoll: Dice={dice} Sides={sides} Sign={signMult}");
			}

			public DiceRoll Roll() {
				if (mod != null && sides != -1) { // If this is null, treat this like an int.
					Random random = new();
					Logging.Debug($"Rolling {dice}d{sides}...");
					rolls = new List<string>();
					int result = 0;
					for (int i = 0; i < dice; i++) {
						int roll = random.Next(sides) + 1;
						result += roll * signMult;
						rolls.Add((signMult * roll).ToString());
					}
					mod.Roll().GetStringResult().ForEach(res => rolls.Add(res));
					total = result + mod.GetIntTotal();
				} else if(mod != null){
					Logging.Debug($"Handling integer modifier:  Dice={dice} Sides={sides} Sign={signMult}");
					total = (dice * signMult) + mod.Roll().GetIntTotal();
					rolls.Add((dice * signMult).ToString());
					mod.GetStringResult().ForEach(res => rolls.Add(res));
				} else {
					total = dice * signMult;
					rolls.Add((dice * signMult).ToString());
				}
				return this;
			}

			public List<string> GetStringResult() {
				return rolls;
			}

			public int GetIntTotal() {
				return total;
			}
		}
	}
}
