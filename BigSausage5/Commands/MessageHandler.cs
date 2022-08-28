using BigSausage.Commands;
using BigSausage.IO;
using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using System.Reflection;

namespace BigSausage {
    public class MessageHandler {

        public static readonly string BOT_PREFIX = "!bs ";
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly List<SlashCommandBuilder> _slashCommandBuilders;

        public MessageHandler(DiscordSocketClient client, CommandService commands) {
            Logging.Info("Initializing CommandHandler...");
            _client = client;
            _commands = commands;
            _slashCommandBuilders = new List<SlashCommandBuilder>();
        }

        public async Task InitGlobalSlashCommands(DiscordSocketClient? client) {
            Logging.Verbose("Initializing global slash commands...");
            if (client != null) {
                var l10n = BigSausage.GetLocalizationManager(client);

                Logging.Debug("Initializing bs-ping...");
                _slashCommandBuilders.Add(new SlashCommandBuilder().WithName("bs-ping").WithDescription(l10n.GetLocalizedString("en_US", "command_ping_description")));

                Logging.Debug("Initializing bs-help...");
                _slashCommandBuilders.Add(new SlashCommandBuilder().WithName("bs-help").WithDescription(l10n.GetLocalizedString("en_US", "command_help_general"))
                    .WithDefaultMemberPermissions(GuildPermission.SendMessages));

                Logging.Debug("Initializing bs-tts...");
                _slashCommandBuilders.Add(new SlashCommandBuilder().WithName("bs-tts").WithDescription(l10n.GetLocalizedString("en_US", "command_TTS_description"))
                    .WithDefaultMemberPermissions(GuildPermission.SendTTSMessages));



                try {
                    Logging.Debug("Injecting slash commands...");
                    foreach (SlashCommandBuilder slashCommandBuilder in _slashCommandBuilders) {
                        Logging.Debug($"Injecting {slashCommandBuilder.Name}...");
                        await client.CreateGlobalApplicationCommandAsync(slashCommandBuilder.Build());
                    }
                } catch (HttpException ex) {
                    Logging.LogException(ex, "Error processing slash command!");
                }
                return;
            } else {
                Logging.Critical("Client is null! Cannot Initialize slash commands.");
                return;
            }
        }

        public async Task SetupAsync() {
            Logging.Info("Setting up CommandHandler...");
            _client.MessageReceived += HandleCommandsAsync;

            _commands.AddTypeReader(typeof(bool), new BooleanTypeReader());

            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), services: null);
        }

        public async Task HandleSlashCommandsAsync(SocketSlashCommand command) {
            await command.RespondAsync(SlashCommandManager.HandleSlashCommand(command));
        }

        public async Task HandleCommandsAsync(SocketMessage messageParam) {
            if (messageParam is not SocketUserMessage message) return;
            int argPos = 0;
            var context = new SocketCommandContext(_client, message);

            if ((!message.HasStringPrefix(BOT_PREFIX, ref argPos, StringComparison.OrdinalIgnoreCase))) {
                if (message.Author.IsBot) return;
                Logging.Debug("Message is not a command! Scanning for triggers...");
                HandleTriggers(messageParam);
                return;
            }
            Auditor.LogCommandFinished(context, await _commands.ExecuteAsync(context, argPos, null, MultiMatchHandling.Best));
        }

        private void HandleTriggers(SocketMessage messageParam) {
            if (messageParam.Channel is not null) {
                IGuild guild = (messageParam.Channel as SocketGuildChannel)!.Guild;
                List<Linkable> linkables = Linkables.ScanMessageForLinkableTriggers(guild, messageParam.Author, messageParam.Content);
                if (linkables.Count == 0) return;
                ITextChannel textChannel = (ITextChannel)messageParam.Channel;
                IGuildUser? user = messageParam.Author as IGuildUser;
                if (user != null) {
                    IVoiceChannel? channel = user.VoiceChannel;
                    Linkables.SendLinkables(linkables, textChannel, channel);
                }
            }
        }
    }
}
