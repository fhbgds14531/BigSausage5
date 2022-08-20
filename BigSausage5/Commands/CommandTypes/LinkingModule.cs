using BigSausage.IO;
using Discord;
using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BigSausage.Commands.CommandTypes {
	public class LinkingModule : ModuleBase<SocketCommandContext> {
		
		[Command("list")]
		public async Task List([Remainder] params string[] type) {
			Logging.Log("Listing files...", LogSeverity.Debug);
			bool listImages = false;
			bool listAudio = false;
			if (type == null) {
				Logging.Log("List type was null!", LogSeverity.Debug);
				type = new string[] { "all" };
			}
			switch (type[0]) {
				case "":
				case "all":
				case null:
					listAudio = true;
					listImages = true;
					break;
				case "images":
				case "image":
					listImages = true;
					break;
				case "voice":
				case "audio":
				case "sound":
				case "sounds":
					listAudio = true;
					break;
				default:
					await Utils.ReplyToMessageFromCommand(Context, $"Unrecognized type \"{type}\". Valid types are \"images\" or \"audio\", or nothing for all types.");
					return;
			}

			List<string> images = new();
			List<string> audio = new();


			Logging.Log("Getting linkables for listing...", LogSeverity.Debug);
			List<Linkable> linkables = Linkables.GetLinkablesForGuild(Context.Guild);
			foreach (Linkable linkable in linkables) {
				if (linkable.type == EnumLinkableType.Image && listImages) {
					images.Add(linkable.Name);
				}
				if (linkable.type == EnumLinkableType.Audio && listAudio) {
					audio.Add(linkable.Name);			
				}
			}

			List<string> finalLines = new();

			if (listImages) {
				finalLines.Add("Images:");
				finalLines.Add($"```{Utils.FormatListItems(images)}```");
			}
			if (listAudio) {
				finalLines.Add("Audio Clips:");
				finalLines.Add($"```{Utils.FormatListItems(audio)}```");
			}

			List<string> messages = new();
			string output = "";
			finalLines.ForEach(line => output += line);

			messages = Utils.EnforceCharacterLimit(new List<string>() { output });

			foreach (string message in messages) {
				await Utils.ReplyToMessageFromCommand(Context, message);
			}

			return;
		}


		[Command("image")]
		public async Task Image([Remainder] string args) {
			await Task.CompletedTask;
		}

		[Command("voice")]
		public async Task Voice([Remainder] string args) {
			await Task.CompletedTask;
		}

		[Command("upload")]
		public async Task Upload([Remainder] string triggerStrings = "") {
			Logging.Debug("Upload requested!");
			if(Permissions.Permissions.UserMeetsPermissionRequirements(Context, Permissions.EnumPermissionLevel.High)) {
				Logging.Debug("User has permission!");
				Attachment[] attachments = Context.Message.Attachments.ToArray();
				Logging.Verbose($"Upload command with {attachments.Length} attachments received! Processing attachments...");
				foreach (Attachment attachment in attachments) {
					Logging.Debug($"\"{attachment.Filename}\"...");
					_ = await DownloadAndAddLinkable(Context.Guild, attachment, triggerStrings.Split(" "));
				}
			} else {
				Logging.Warning("User did not meet permissions");
				await Utils.SendNoPermissionReply(Context);
			}
		}

		private async Task<bool> DownloadAndAddLinkable(IGuild guild, Attachment attachment, string[] triggerStrings) {
			EnumLinkableType type = EnumLinkableType.Audio;
			string saveDir = "";
			string stringType = "null";
			EnumAttachmentValidation validation = ValidateAttachment(attachment);
			switch (validation) {
				case EnumAttachmentValidation.InvalidSize:
					Logging.Debug("Invalid size! Skipping...");
					await Utils.ReplyToMessageFromCommand(Context, "Sorry, your attachment (" + attachment.Filename + ") is too large! Please keep uploads under 5Mb.");
					return false;
				case EnumAttachmentValidation.InvalidType:
					Logging.Verbose("Invalid type! Skipping...");
					await Utils.ReplyToMessageFromCommand(Context, "Sorry, your attachment (" + attachment.Filename + ") is of an unrecognized type! Only images and WAV files are supported.");
					return false;
				case EnumAttachmentValidation.InvalidTypeImage:
					Logging.Verbose("Invalid image type! Skipping...");
					await Utils.ReplyToMessageFromCommand(Context, $"Sorry, your attachment ({attachment.Filename}) is of an invalid type ({attachment.ContentType})! Images must be of type `jpg`, `jpeg`, `png`, `bmp`, or `gif`");
					return false;
				case EnumAttachmentValidation.InvalidTypeAudio:
					Logging.Verbose("Invalid audio type! Skipping...");
					await Utils.ReplyToMessageFromCommand(Context, "Sorry, your attachment (" + attachment.Filename + ") could not be accepted. Audio files must be of the WAV format.");
					return false;
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
			Logging.Info($"{stringType} upload request from guild \"{Context.Guild.Name}\" ({guildID}) accepted! Downloading {attachment.Filename} to disk...");
			IO.IOUtilities.DownloadFile(attachment.Url, filename);
			Linkable lkb = new(lName, guildID, filename, type, triggerStrings);
			Linkables.AddLinkableToGuild(Context.Guild, lkb);
			Logging.Debug("Attachment downloaded and added!");
			await Utils.ReplyToMessageFromCommand(Context, $"Successfully added {lName}!");
			return true;
		}

		private EnumAttachmentValidation ValidateAttachment(Attachment attachment) {
			if (attachment.Size < 4_999_999) {
				string type = attachment.ContentType;
				if (type.StartsWith("audio")) {
					if(type.EndsWith("wav")) return EnumAttachmentValidation.ValidWav;
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
