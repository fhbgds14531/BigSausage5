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
	public class LinkableModule : ModuleBase<SocketCommandContext> {
		
		[Command("list")]
		public async Task List([Remainder] params string[] type) {
			Logging.Debug("Listing files...");
			bool listImages = false;
			bool listAudio = false;
			if (type == null || type.Length == 0) {
				Logging.Debug("List type was null!");
				type = new string[] { "all" };
			}
			switch (type[0]) {
				case "":
				case "all":
				case null:
					Logging.Debug("Interpereted command as !bs list all");
					listAudio = true;
					listImages = true;
					break;
				case "images":
				case "image":
					Logging.Debug("Interpereted command as !bs list images");
					listImages = true;
					break;
				case "voice":
				case "audio":
				case "sound":
				case "sounds":
					Logging.Debug("Interpereted command as !bs list audio");
					listAudio = true;
					break;
				default:
					await Utils.ReplyToMessageFromCommand(Context, $"Unrecognized type \"{type}\". Valid types are \"images\" or \"audio\", or nothing for all types.");
					return;
			}

			List<string> images = new();
			List<string> audio = new();


			Logging.Debug("Getting linkables for listing...");
			List<Linkable> linkables = Linkables.GetLinkablesForGuild(Context.Guild);
			if(linkables != null && linkables.Count > 0) {
				foreach (Linkable linkable in linkables) {
					if (linkable.type == EnumLinkableType.Image && listImages) {
						if (linkable.Name != null) {
							images.Add(linkable.Name);
						} else {
							Logging.Critical("Linkable has a null name, have things happened out of order or did loading fail?");
						}
					}
					if (linkable.type == EnumLinkableType.Audio && listAudio) {
						if (linkable.Name != null) {
							audio.Add(linkable.Name);
						} else {
							Logging.Critical("Linkable has a null name, have things happened out of order or did loading fail?");
						}
					}
				}
			}

			List<string> finalLines = new();

			if (listImages && images.Count > 0) {
				Logging.Debug($"Formatting {images.Count} images...");
				finalLines.Add("Images:");
				finalLines.Add($"```{Utils.FormatListItems(images)}```");
			}
			if (listAudio && audio.Count > 0) {
				Logging.Debug($"Formatting {audio.Count} audio clips...");
				finalLines.Add("Audio Clips:");
				finalLines.Add($"```{Utils.FormatListItems(audio)}```");
			}

			List<string> messages = new();
			string output = "";
			Logging.Debug("Converting lists to output string...");
			finalLines.ForEach(line => output += line);

			Logging.Debug("Enforcing character limit on reply...");
			messages = Utils.EnforceCharacterLimit(new List<string>() { output });

			foreach (string message in messages) {
				await Utils.ReplyToMessageFromCommand(Context, message);
			}

			return;
		}


		[Command("image")]
		public async Task Image([Remainder] string names = "") {
			Logging.Verbose("Image requested!");
			List<Linkable> links = new();
			if (names != null && names.Length > 0) {
				Logging.Verbose($"Image linker has been provided with the following string: {names}");
				foreach (string name in names.Split(" ")) {
					Linkables.GetLinkablesByNameOrTrigger(Context.Guild, name).ForEach(linkable => links.Add(linkable));
				}
				Logging.Verbose($"Linkable search resulted in {links.Count} match{(links.Count == 1 ? "" : "es")}!");
			} else {
				Logging.Verbose("Image linker was not provided with a query, it will instead choose a random image...");
				List<Linkable> guildLinks = Linkables.GetLinkablesForGuild(Context.Guild);
				links.Add(guildLinks[new Random().Next(guildLinks.Count)]);
			}
			foreach (Linkable linkable in links) {
				if (linkable.Filename != null) {
					await Context.Channel.SendFileAsync(new FileAttachment(File.OpenRead(linkable.Filename), linkable.Filename[linkable.Filename.LastIndexOf("\\")..]));
				}
			}
			await Task.CompletedTask;
		}

		[Command("voice")]
		public async Task Voice([Remainder] string names) {
			if (names != null && names.Length > 0) {

			}
			await Task.CompletedTask;
		}

		[Command("upload")]
		public async Task Upload([Remainder] string triggerStrings = "") {
			await Utils.ReplyToMessageFromCommand(Context, Linkables.HandleUpload(Context.Guild, Context.Message.Attachments.ToArray(), Context.Message.Author, 
				triggerStrings.Split("")));


		}


	}
}
