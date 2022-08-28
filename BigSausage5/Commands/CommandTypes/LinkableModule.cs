using BigSausage.IO;
using Discord;
using Discord.Commands;

namespace BigSausage.Commands.CommandTypes {
	public class LinkableModule : ModuleBase<SocketCommandContext> {

		[Command("list", RunMode = RunMode.Async)]
		public async Task List([Remainder] string type = "") {
			Logging.Debug("Listing files...");
			bool listImages = false;
			bool listAudio = false;
			switch (type) {
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
			if (linkables != null && linkables.Count > 0) {
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


		public async Task Remove(string name) {
			if (name != null) {
				if (Permissions.Permissions.GetUserPermissionLevelInGuild(Context.Guild, Context.User) >= Permissions.EnumPermissionLevel.High) {
					List<Linkable> linkables = Linkables.GetLinkablesByNameOrTrigger(Context.Guild, name);

				}
			}
		}


		[Command("image", RunMode = RunMode.Async)]
		public async Task Image([Remainder] string names = "") {
			Logging.Verbose("Image requested!");
			List<Linkable> links = new();
			if (names != null && names.Length > 0) {
				Logging.Verbose($"Image linker has been provided with the following string: {names}");

				int i = 0;
				if (int.TryParse(names, out i)) {
					Logging.Debug("String is an integer!");
					if (i > 0) {
						List<Linkable> images = Linkables.GetLinkablesOfTypeForGuild(Context.Guild, EnumLinkableType.Image);
						Random rand = new();
						for (int count = 0; count < i; count++) {
							Logging.Debug("Getting a random image...");
							int index = rand.Next(images.Count) - 1;
							Linkable image = images[index];
							images.Remove(image);
							links.Add(image);
						}
					} else {
						await Utils.ReplyToMessageFromCommand(Context, "When providing a number, the number must be a positive integer.");
						return;
					}
				} else {
					foreach (string name in names.Split(" ")) {
						links.AddRange(Linkables.GetLinkablesOfTypeByNameOrTrigger(Context.Guild, name, EnumLinkableType.Image));
					}
					Logging.Verbose($"Linkable search resulted in {links.Count} match{(links.Count == 1 ? "" : "es")}!");
				}
			} else {
				Logging.Verbose("Image linker was not provided with a query, it will instead choose a random image...");
				List<Linkable> guildLinks = Linkables.GetLinkablesOfTypeForGuild(Context.Guild, EnumLinkableType.Image);
				links.Add(guildLinks[new Random().Next(guildLinks.Count)]);
			}
			foreach (Linkable linkable in links) {
				if (linkable.Filename != null) {
					await Context.Channel.SendFileAsync(new FileAttachment(File.OpenRead(linkable.Filename), linkable.Filename[linkable.Filename.LastIndexOf("\\")..]));
				}
			}
			await Task.CompletedTask;
		}

		[Command("voice", RunMode = RunMode.Async)]
		public async Task Voice([Remainder] string names = "") {
			Logging.Verbose("Audio requested!");
			List<Linkable> links = new();
			if (names != null && names.Length > 0) {
				Logging.Verbose($"Audio linker has been provided with the following string: {names}");
				int i = 0;
				if (int.TryParse(names, out i)) {
					Logging.Debug("String is an integer!");
					if (i > 0) {
						List<Linkable> audio = Linkables.GetLinkablesOfTypeForGuild(Context.Guild, EnumLinkableType.Audio);
						Random rand = new();
						for (int count = 0; count < i; count++) {
							int index = rand.Next(audio.Count) - 1;
							Linkable clip = audio[index];
							audio.Remove(clip);
							links.Add(clip);
						}
					} else {
						await Utils.ReplyToMessageFromCommand(Context, "When providing a number, the number must be a positive integer.");
						return;
					}
				} else {
					foreach (string name in names.Split(" ")) {
						links.AddRange(Linkables.GetLinkablesOfTypeByNameOrTrigger(Context.Guild, name, EnumLinkableType.Audio));
					}
				}
				Logging.Verbose($"Linkable search resulted in {links.Count} match{(links.Count == 1 ? "" : "es")}!");
			} else {
				Logging.Verbose("Audio linker was not provided with a query, it will instead choose a random clip...");
				List<Linkable> guildLinks = Linkables.GetLinkablesOfTypeForGuild(Context.Guild, EnumLinkableType.Audio);
				links.Add(guildLinks[new Random().Next(guildLinks.Count)]);
			}
			Logging.Debug("Attempting to send audio...");
			IVoiceChannel channel = AudioManager.GetVoiceChannelFromSocketUser(Context.User);
			foreach (Linkable linkable in links) {
				if (linkable.Filename != null) {
					BigSausage.GetAudioManager().AddClipToQueue(Context.Guild, linkable.Filename);
				}
			}
			Logging.Debug("Playing queued files...");
			await BigSausage.GetAudioManager().SafePlayFiles(Context.Guild, channel);
			await Task.CompletedTask;
		}

		[Command("upload")]
		public async Task Upload([Remainder] string triggerStrings = "") {
			bool alreadyExists = false;

			if (alreadyExists) {
				await Utils.ReplyToMessageFromCommand(Context, "A linkable with that name already exists! Please try again with a different filename.");
				return;
			} else {
				await Utils.ReplyToMessageFromCommand(Context, Linkables.HandleUpload(Context.Guild, Context.Message.Attachments.ToArray(), Context.Message.Author,
					triggerStrings.Split("")));
			}

		}


	}
}
