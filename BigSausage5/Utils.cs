using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Xml.Serialization;
using System.Xml;

namespace BigSausage {
	public class Utils {

		public static string GetProcessPathDir() {
			if (Environment.ProcessPath != null) {
				return Environment.ProcessPath.Replace("\\BigSausage5.exe", "");
			} else {
				Logging.Log("Failed to get ProcessPath! Defaulting to Desktop.", Discord.LogSeverity.Error);
				Logging.LogErrorToFile(null, null, "Failed to get ProcessPath!");
				return Environment.GetFolderPath(Environment.SpecialFolder.Desktop) + "\\BigSausage Fallback Directory";
			}
		}

		public static async Task ReplyToMessageFromCommand(SocketCommandContext context, string reply) {
			Discord.MessageReference message = new(context.Message.Id, context.Channel.Id, context.Guild.Id);
			await context.Channel.SendMessageAsync(reply, false, null, null, null, message, null, null, null);
		}

		public static async Task SendNoPermissionReply(SocketCommandContext context) {
			await ReplyToMessageFromCommand(context, "Sorry! You don't have permission to use that command! :(");
		}
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
