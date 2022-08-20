using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;
using System.Text.RegularExpressions;
using Discord;

namespace BigSausage {
	public class Utils {


		readonly static int _characterLimit = 2000;

		public static List<string> LimitToFirstNItems(List<string> items, int n) {
			//List<string> result = new();
			throw new NotImplementedException();
		}

		public static List<string> EnforceCharacterLimit(List<string> messages) {
			Logging.Log($"Enforcing character limit for {messages.Count} messages...", Discord.LogSeverity.Debug);
			List<string> result = new();

			foreach (string message in messages) {
				if (message.Length >= _characterLimit) {
					SplitMessageFormattingAware(message).ForEach((s) => result.Add(s));
				} else {
					result.Add(message);
				}
			}

			return result;
		}

		public static List<string> SplitMessageFormattingAware(string message) {
			List<string> result = new();


			Regex matcher = new("(```[\\w*\\,\\s]*```)|(`[\\w*\\ ]*`)|(\\*+[\\w\\ ]+\\*+)");

			int messageOverstep = message.Length - _characterLimit;

			if (matcher.IsMatch(_testString)) {
				Match match = matcher.Match(_testString);
				int count = 0;
				foreach (Group group in match.Groups) {
					Logging.Log($"Test string groups: {count}={match.Groups[count++]}", Discord.LogSeverity.Debug);
				}
			}

			result.Add(message);
			return result;
		}

		public static string FormatListItems(List<string> list, int itemsPerLine = 8) {
			string output = "";
			if(itemsPerLine < 1) itemsPerLine = 1;
			int count = 0;
			foreach (string item in list) {
				output += item;
				count++;
				if (count >= itemsPerLine) {
					output += "\n";
					count = 0;
				} else {
					output += ", ";
				}
			}
			return output[0..^2];
		}

		public static string GetProcessPathDir() {
			if (Environment.ProcessPath != null) {
				return Environment.ProcessPath.Replace("\\BigSausage5.exe", "");
			} else {
				Logging.Log("Failed to get ProcessPath! Defaulting to Desktop.", Discord.LogSeverity.Error);
				Logging.LogErrorToFile(null, null, "Failed to get ProcessPath!");
				return Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\BigSausage Fallback Directory";
			}
		}

		public static string GetGuildLinkableDirectory(IGuild guild) {
			return GetProcessPathDir() + $"\\Files\\Guilds\\{guild.Id}\\Linkables";
		}

		public static async Task ReplyToMessageFromCommand(SocketCommandContext context, string reply) {
			Discord.MessageReference message = new(context.Message.Id, context.Channel.Id, context.Guild.Id);
			await context.Channel.SendMessageAsync(reply, false, null, null, null, message, null, null, null);
		}

		public static async Task SendNoPermissionReply(SocketCommandContext context) {
			await ReplyToMessageFromCommand(context, "Sorry! You don't have permission to use that command! :(");
		}

		private static readonly string _testString = "Images:\n```image1, image2, image3, image4, image5, image6\nimage7, image8, image9```\n \nAudio Clips:\n```clip1, clip2, clip3, clip4\nclip5, clip6, clip7```\n \nImages2:\n```test, test, test2```\n\n*bold text*\n**italics**\n\n`small quote`\n\n`\n\n`\n\n";
	}

	

	[XmlRoot("dictionary")]
#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
	public class SerializableDictionary<TKey, TValue>
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
	: Dictionary<TKey, TValue>, IXmlSerializable {
		public SerializableDictionary() { }
		public SerializableDictionary(IDictionary<TKey, TValue> dictionary) : base(dictionary) { }
		public SerializableDictionary(IDictionary<TKey, TValue> dictionary, IEqualityComparer<TKey> comparer) : base(dictionary, comparer) { }
		public SerializableDictionary(IEqualityComparer<TKey> comparer) : base(comparer) { }
		public SerializableDictionary(int capacity) : base(capacity) { }
		public SerializableDictionary(int capacity, IEqualityComparer<TKey> comparer) : base(capacity, comparer) { }

		#region IXmlSerializable Members
		public System.Xml.Schema.XmlSchema GetSchema() {
			return new System.Xml.Schema.XmlSchema();
		}

		public void ReadXml(System.Xml.XmlReader reader) {
			XmlSerializer keySerializer = new(typeof(TKey));
			XmlSerializer valueSerializer = new(typeof(TValue));

			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();

			if (wasEmpty)
				return;

			while (reader.NodeType != System.Xml.XmlNodeType.EndElement) {
				reader.ReadStartElement("item");

				reader.ReadStartElement("key");
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
				TKey key = (TKey)keySerializer.Deserialize(reader);
				reader.ReadEndElement();

				reader.ReadStartElement("value");
				TValue value = (TValue)valueSerializer.Deserialize(reader);
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
				reader.ReadEndElement();

#pragma warning disable CS8604 // Possible null reference argument.
				this.Add(key, value);
#pragma warning restore CS8604 // Possible null reference argument.

				reader.ReadEndElement();
				reader.MoveToContent();
			}
			reader.ReadEndElement();
		}

		public void WriteXml(System.Xml.XmlWriter writer) {
			XmlSerializer keySerializer = new(typeof(TKey));
			XmlSerializer valueSerializer = new(typeof(TValue));

			foreach (TKey key in this.Keys) {
				writer.WriteStartElement("item");

				writer.WriteStartElement("key");
				keySerializer.Serialize(writer, key);
				writer.WriteEndElement();

				writer.WriteStartElement("value");
				TValue value = this[key];
				valueSerializer.Serialize(writer, value);
				writer.WriteEndElement();

				writer.WriteEndElement();
			}
		}
		#endregion
	}
}
