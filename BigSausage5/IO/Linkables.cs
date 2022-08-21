using BigSausage.Commands;
using Discord;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BigSausage.IO {
	internal class Linkables {

		private static SerializableDictionary<ulong, List<Linkable>>? _linkables;
		private static bool _initialized = false;

		public static void Initialize() {
			if (!_initialized) {
				Logging.Info("Initializing Linkables...");
				_linkables = IO.IOUtilities.LoadLinkablesFromDisk(BigSausage.GetClient());


			} else {
				Logging.Warning("Linkable initialization was requested but linkables have already been initialized! ignoring...");
			}
			_initialized = true;
		}

		public static void AddLinkableToGuild(IGuild guild, Linkable linkable) {
			if (!_initialized) {
				Logging.Warning("Adding a Linkable was requested but Linkables have not yet been initialized!");
				Initialize();
			}
			if (_linkables != null) {
				_linkables[guild.Id].Add(linkable);
				IO.IOUtilities.SaveLinkablesToDisk(BigSausage.GetClient(), _linkables);
			} else {
				Logging.Critical("Linkables have been initialized but are still null!");
			}
		}

		public static void Save() {
			if (_initialized && _linkables != null) {
				Logging.Debug("Saving linkables...");
				IO.IOUtilities.SaveLinkablesToDisk(BigSausage.GetClient(), _linkables);
			} else {
				Logging.Warning("Linkables were never properly initialized, so they will not be saved.");
			}
		}

		public static List<Linkable> GetLinkablesForGuild(IGuild guild) {
			if (!_initialized) {
				Logging.Warning($"Linkables for guild \"{guild.Name}\"({guild.Id}) requested but linkables have not been initialized!");
				Initialize();
			}
			if (_linkables == null) {
				IOException e = new("Linkables initialization failed!");
				Logging.LogException(e, "_linkables was null post initialization!");
				_linkables = new();
			}
			List<Linkable> linkables = new();

			if (_linkables.ContainsKey(guild.Id)) {
				linkables = _linkables[guild.Id];
			} else {
				Logging.Error($"Linkables is missing an entry for guild \"{guild.Name}\"({guild.Id})! Adding an empty entry...");
				_linkables[guild.Id] = new();
			}

			return linkables;
		}

		public static List<Linkable> GetLinkablesByNameOrTrigger(IGuild guild, string nameOrTrigger) {
			Logging.Verbose($"Checking guild \"{guild.Name}\" ({guild.Id}) for linkables using key \"{nameOrTrigger}\"");
			List<Linkable> result = new();
			if (!_initialized) {
				Logging.Warning("Linkable requested but linkables have not yet been initialized!");
				Initialize();
			}
			if(_linkables != null) {
				_linkables[guild.Id].ForEach(linkable => { if (linkable.Triggers.Contains(nameOrTrigger) || linkable.Name == nameOrTrigger) result.Add(linkable);});
			}
			Logging.Verbose($"Found {(result.Count == 1 ? (result.Count + " linkable") : result.Count + " linkables")}!");
			return result;

		}

		public static List<Linkable> ScanMessageForLinkableTriggers(IGuild guild, string message) {
			if (!_initialized) {
				Logging.Warning("Trigger parsing was requested but Linkables have not yet been initialized!");
				Initialize();
			}
			if(_linkables != null) {
				List<Linkable> result = new();
				string[] split = message.Split(' ');
				foreach (string word in split) {
					foreach (Linkable linkable in _linkables[guild.Id]) {
						if (linkable.Triggers == null) {
							Logging.Error("Triggers for linkable are null! " + linkable.Name);
						} else {
							foreach (string trigger in linkable.Triggers) {
								if (trigger == word) {
									result.Add(linkable);
									break;
								}
							}
						}
					}
				}
				return result;
			} else {
				Logging.Critical("Linkables were initialized but are still null!");
				return new();
			}
		}

		public static string HandleUpload(IGuild guild, Attachment[] attachments, IUser user, params string[] triggers) {
			Logging.Debug("Upload requested!");
			if (Permissions.Permissions.UserMeetsPermissionRequirements(guild, user, Permissions.EnumPermissionLevel.High)) {
				Logging.Debug("User has permission!");
				Logging.Verbose($"Upload command with {attachments.Length} attachment{(attachments.Length == 1 ? "" : "s")} received! Processing attachments...");
				List<string> responses = new();
				foreach (Attachment attachment in attachments) {
					Logging.Debug($"\"{attachment.Filename}\"...");
					responses.Add(DownloadAndAddLinkable(guild, attachment, triggers));
				}
				string result = "";
				foreach (string response in responses) {
					result += response + "\n";
				}
				return result.Trim();
			} else {
				Logging.Warning("User did not meet permissions");
				return "Sorry, you don't have permission to use that command!";
			}
		}

		private static string DownloadAndAddLinkable(IGuild guild, Attachment attachment, string[] triggerStrings) {
			EnumLinkableType type = EnumLinkableType.Audio;
			string saveDir = "";
			string stringType = "null";
			EnumAttachmentValidation validation = ValidateAttachment(attachment);
			switch (validation) {
				case EnumAttachmentValidation.InvalidSize:
					Logging.Debug("Invalid size! Skipping...");
					return "Sorry, your attachment (" + attachment.Filename + ") is too large! Please keep uploads under 5Mb.";
				case EnumAttachmentValidation.InvalidType:
					Logging.Verbose("Invalid type! Skipping...");
					return "Sorry, your attachment (" + attachment.Filename + ") is of an unrecognized type! Only images and WAV files are supported.";
				case EnumAttachmentValidation.InvalidTypeImage:
					Logging.Verbose("Invalid image type! Skipping...");
					return $"Sorry, your attachment ({attachment.Filename}) is of an invalid type ({attachment.ContentType})! Images must be of type `jpg`, `jpeg`, `png`, `bmp`, or `gif`";
				case EnumAttachmentValidation.InvalidTypeAudio:
					Logging.Verbose("Invalid audio type! Skipping...");
					return "Sorry, your attachment (" + attachment.Filename + ") could not be accepted. Audio files must be of the WAV format.";
				case EnumAttachmentValidation.ValidWav:
					Logging.Verbose("Wav file validated! Gathering data...");
					type = EnumLinkableType.Audio;
					saveDir = Utils.GetGuildLinkableDirectory(guild) + "\\Audio\\";
					stringType = "Audio";
					break;
				case EnumAttachmentValidation.ValidImage:
					Logging.Verbose("Image file validated! Gathering data...");
					type = EnumLinkableType.Image;
					saveDir = Utils.GetGuildLinkableDirectory(guild) + "\\Images\\";
					stringType = "Image";
					break;
			}

			var lName = attachment.Filename[..attachment.Filename.LastIndexOf(".")];
			var guildID = guild.Id.ToString();
			var filename = saveDir + attachment.Filename;
			Logging.Debug($"Data acquisition complete! Type:{stringType}, Name:{lName}");

			if (!triggerStrings.Contains(lName)) _ = triggerStrings.Append(lName);
			Logging.Info($"{stringType} upload request from guild \"{guild.Name}\" ({guildID}) accepted! Downloading {attachment.Filename} to disk...");
			IO.IOUtilities.DownloadFile(attachment.Url, filename);
			Linkable lkb = new(lName, guildID, filename, type, triggerStrings);
			AddLinkableToGuild(guild, lkb);
			Logging.Debug("Attachment downloaded and added!");
			return $"Successfully added {lName}!";
		}

		public static void MigrateLegacyLinkable(string path) {
			EnumLinkableType? type = null;
			EnumAttachmentValidation validation = ValidateLegacyFile(path);
			switch (validation) {
				case EnumAttachmentValidation.ValidWav:
					Logging.Warning("File is a valid .wav file!");
					type = EnumLinkableType.Audio;
					break;
				case EnumAttachmentValidation.ValidImage:
					Logging.Warning("File is a valid image!");
					type = EnumLinkableType.Image;
					break;
				default:
					Logging.Warning("File is not a valid linkable, it will be skipped!");
					return;
			}
			if(type != null) {
				string filename = path.Substring(path.LastIndexOf(@"\") + 1);
				string guildID = path.Replace(@"\" + filename, "");
				guildID = guildID.Substring(guildID.LastIndexOf(@"\") + 1);
				string name = filename.Replace(filename.Substring(filename.LastIndexOf(".")), "");
				string[] triggers = { name };
				IGuild guild = BigSausage.GetClient().GetGuild(ulong.Parse(guildID));
				Logging.Warning("Legacy linkable data collection complete! Migrating...");

				filename = Utils.GetProcessPathDir() + $"\\Files\\Guilds\\{guildID}\\Linkables\\{(type == EnumLinkableType.Audio ? "Audio" : "Images")}\\" + filename;
				File.Move(path, filename);

				AddLinkableToGuild(guild, new Linkable(name, guildID, filename, (EnumLinkableType) type, triggers));
				Logging.Warning("Legacy linkable migrated!");
			}
		}

		private static EnumAttachmentValidation ValidateAttachment(Attachment attachment) {
			if (attachment.Size < 4_999_999) {
				string type = attachment.ContentType;
				if (type.StartsWith("audio")) {
					if (type.EndsWith("wav")) return EnumAttachmentValidation.ValidWav;
					return EnumAttachmentValidation.InvalidTypeAudio;
				}
				if (type.StartsWith("image")) {
					Regex images = new("(image\\/(jpg|jpeg|png|bmp|gif))$");
					if (images.IsMatch(type)) return EnumAttachmentValidation.ValidImage;
					return EnumAttachmentValidation.InvalidTypeImage;
				}
				return EnumAttachmentValidation.InvalidType;
			} else {
				return EnumAttachmentValidation.InvalidSize;
			}
		}

		private static EnumAttachmentValidation ValidateLegacyFile(string path) {
			Logging.Warning("Validating legacy file...");
			if (!File.Exists(path))  return EnumAttachmentValidation.InvalidType;
			if (new FileInfo(path).Length > 4_999_999) return EnumAttachmentValidation.InvalidSize;

			if (path.ToLower().EndsWith(".wav")) return EnumAttachmentValidation.ValidWav; 
			
			Regex images = new(@"(.+\.(jpg|jpeg|png|bmp|gif))$");
			if (images.IsMatch(path)) return EnumAttachmentValidation.ValidImage;
			return EnumAttachmentValidation.InvalidType;
		}

		private enum EnumAttachmentValidation {
			InvalidTypeAudio,
			InvalidTypeImage,
			InvalidType,
			InvalidSize,
			ValidWav,
			ValidImage
		}
	}
}
