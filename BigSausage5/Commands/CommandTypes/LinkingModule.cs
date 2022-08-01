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
			Logging.Log("Upload requested!", LogSeverity.Debug);
			if(Permissions.Permissions.UserMeetsPermissionRequirements(Context, Permissions.EnumPermissionLevel.High)) {
				Logging.Log("User has permission!", LogSeverity.Debug);
				Attachment[] attachments = Context.Message.Attachments.ToArray();
				foreach (Attachment attachment in attachments) {
					if (attachment.Size > 4_999_999) {
						await Utils.ReplyToMessageFromCommand(Context, "Sorry, your attachment (" + attachment.Filename +") is too large! Please keep uploads under 5Mb.");
						continue;
					} else {
						if (triggerStrings == String.Empty) triggerStrings = attachment.Filename;
						string[] triggers = triggerStrings.Split(" ");
						Logging.Log("Attachment is under 5Mb!", LogSeverity.Debug);
						string type = attachment.ContentType;
						string? lName;
						string? filename;
						string? lType;
						string? guildID;
						if (type.StartsWith("audio")) {
							Logging.Log("Attachment identified as audio!", LogSeverity.Debug);
							if (!type.EndsWith("wav")) {
								await Utils.ReplyToMessageFromCommand(Context, "Sorry, your attachment (" + attachment.Filename + ") could not be accepted. Audio files must be of the WAV format.");
								continue;
							}
							lType = "audio";
							if (type.EndsWith("wav")) {
								Logging.Log("Attachment identified as a .wav file!", LogSeverity.Debug);
								guildID = Context.Guild.Id.ToString();
								filename = Utils.GetProcessPathDir() + "\\Files\\Guilds\\" + guildID + "\\Linkables\\Audio\\" + attachment.Filename;
								lName = attachment.Filename[..attachment.Filename.LastIndexOf(".")]; 
								if (!triggers.Contains(lName)) _ = triggers.Append(lName);
								Logging.Log("Audio upload request from guild \"" + Context.Guild.Name + "\" (" + guildID + ") accepted! Downloading " +
									attachment.Filename + " to disk...", LogSeverity.Info);
								IO.IOUtilities.DownloadFile(attachment.Url, filename); 
								Linkable lkb = new(lName, guildID, filename, lType, triggers);
								Linkables.AddLinkableToGuild(Context.Guild, lkb);
								Logging.Log("Attachment downloaded and added!", LogSeverity.Debug);
							} else {
								await Utils.ReplyToMessageFromCommand(Context, "Sorry, BigSausage only supports .wav files for audio linking.");
								continue;
							}
						} else if (type.StartsWith("image")) {
							lType = "image";
							Logging.Log("Attachment identified as an image!", LogSeverity.Debug);
							Regex images = new("(image\\/(jpg|jpeg|png|bmp|gif))$");
							if (images.IsMatch(type)) {
								Logging.Log("Attachment file extension verified!", LogSeverity.Debug);
								guildID = Context.Guild.Id.ToString();
								filename = Utils.GetProcessPathDir() + "\\Files\\Guilds\\" + guildID + "\\Linkables\\Images\\" + attachment.Filename;
								lName = attachment.Filename[..attachment.Filename.LastIndexOf(".")];
								if (!triggers.Contains(lName)) _ = triggers.Append(lName);
								Logging.Log("Image upload request from guild \"" + Context.Guild.Name + "\" (" + guildID + ") accepted! Downloading " + 
									attachment.Filename + " to disk...", LogSeverity.Info);
								IO.IOUtilities.DownloadFile(attachment.Url, filename);
								Linkable lkb = new(lName, guildID, filename, lType, triggers);
								Linkables.AddLinkableToGuild(Context.Guild, lkb);
								Logging.Log("Attachment downloaded and added!", LogSeverity.Debug);
							} else {
								await Utils.ReplyToMessageFromCommand(Context, $"Sorry, your attachment ({attachment.Filename}) is of an invalid type ({attachment.ContentType})! Images must be of type `jpg`, `jpeg`, `png`, `bmp`, or `gif`");
								continue;
							}
						} else {
							Logging.Log($"Attachment type ({attachment.ContentType}) is not recognized!", LogSeverity.Warning);
						}
					}
				}
			} else {
				Logging.Log("User did not meet permissions", LogSeverity.Warning);
				await Utils.SendNoPermissionReply(Context);
			}
		}

	}
}
