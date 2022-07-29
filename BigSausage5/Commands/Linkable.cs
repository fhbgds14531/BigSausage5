using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace BigSausage.Commands {
	public class Linkable : IXmlSerializable {

		public string? Filename;

		public string? Type;

		public string? GuildID;

		public string? Name;

		public string[]? Triggers;

		private Linkable() { }

		public Linkable(string name, string guildID, string filename, string type, params string[] triggers) {
			this.Name = name;
			this.GuildID = guildID;
			this.Filename = filename;
			this.Triggers = triggers;
			this.Type = type;
		}

		public XmlSchema? GetSchema() {
			return new XmlSchema();
		}

		public void ReadXml(XmlReader reader) {
			XmlSerializer stringSerializer = new XmlSerializer(typeof(string));

			bool wasEmpty = reader.IsEmptyElement;
			reader.Read();

			if (wasEmpty) return;

			
				reader.ReadStartElement("linkable");
					reader.ReadStartElement("descriptors");
						reader.ReadStartElement("name");
							Name = (string) stringSerializer.Deserialize(reader);
						reader.ReadEndElement();
						reader.ReadStartElement("filename");
							Filename = (string) stringSerializer.Deserialize(reader);
						reader.ReadEndElement();
						reader.ReadStartElement("type");
							Type = (string) stringSerializer.Deserialize(reader);
						reader.ReadEndElement();
						reader.ReadStartElement("guildID");
							GuildID = (string) stringSerializer.Deserialize(reader);
						reader.ReadEndElement();
					reader.ReadEndElement();
					reader.ReadStartElement("triggers");
						List<string> loadedTriggers = new List<string>();
						while (reader.NodeType != System.Xml.XmlNodeType.EndElement) {
							loadedTriggers.Add((string)stringSerializer.Deserialize(reader));
							reader.MoveToContent();
						}
						Triggers = loadedTriggers.ToArray();
					reader.ReadEndElement();
				reader.ReadEndElement();
			reader.ReadEndElement();
		}

		public void WriteXml(XmlWriter writer) {
			XmlSerializer stringSerializer = new XmlSerializer(typeof(string));

			writer.WriteStartElement("linkable");
				writer.WriteStartElement("descriptors");
					writer.WriteStartElement("name");
						stringSerializer.Serialize(writer, Name);
					writer.WriteEndElement();
					writer.WriteStartElement("filename");
						stringSerializer.Serialize(writer, Filename);
					writer.WriteEndElement();
					writer.WriteStartElement("type");
						stringSerializer.Serialize(writer, Type);
					writer.WriteEndElement();
					writer.WriteStartElement("guildID");
						stringSerializer.Serialize(writer, GuildID);
					writer.WriteEndElement();
				writer.WriteEndElement();
				writer.WriteStartElement("triggers");
					if (Triggers != null) {
						foreach (string trigger in Triggers) {
							stringSerializer.Serialize(writer, trigger);
						}
					}
				writer.WriteEndElement();
			writer.WriteEndElement();
		}
	}
}
