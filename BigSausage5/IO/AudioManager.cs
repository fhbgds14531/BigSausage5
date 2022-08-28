using Discord;
using Discord.Audio;
using Discord.WebSocket;
using System.Diagnostics;

namespace BigSausage.IO {
    public class AudioManager {

        private Dictionary<ulong, List<string>> _queue = null!;
        private Dictionary<ulong, bool> _playingInGuild = null!;

        public AudioManager() {
            Logging.Info("Initializing AudioManager...");
            _playingInGuild = new();
            _queue = new();
        }

        public static IVoiceChannel GetVoiceChannelFromSocketUser(SocketUser user) {
            Logging.Debug("Getting voice channel from user...");
            return (user as IGuildUser)!.VoiceChannel;
        }

        public static async Task<IAudioClient> JoinVoiceChannel(IVoiceChannel channel) {
            Logging.Debug("Joining voice channel...");
            return await channel.ConnectAsync(false, false, false);
        }

        public static async Task LeaveVoiceChannel(IVoiceChannel channel) {
            Logging.Debug("Leaving voice channel...");
            await channel.DisconnectAsync();
        }

        private static Process CreateStream(string path) {
            Logging.Debug("Creating stream process...");
            return Process.Start(new ProcessStartInfo {
                FileName = "ffmpeg",
                Arguments = $"-hide_banner -loglevel panic -i \"{path}\" -ac 2 -f s16le -ar 48000 pipe:1",
                UseShellExecute = false,
                RedirectStandardOutput = true,
            })!;
        }

        public void AddClipToQueue(IGuild guild, string filePath) {
            if (!_playingInGuild.ContainsKey(guild.Id)) _playingInGuild[guild.Id] = false;
            if (!_queue.ContainsKey(guild.Id)) _queue[guild.Id] = new();
            _queue[guild.Id].Add(filePath);
        }

        public async Task SafePlayFiles(IGuild guild, IVoiceChannel channel) {
            if (!_playingInGuild[guild.Id]) {
                var client = await JoinVoiceChannel(channel);
                await PlayQueuedFilesInGuildAsync(guild, client);
                Logging.Debug("Finished playing queued files!");
                await LeaveVoiceChannel(channel);
            }
        }

        private async Task PlayQueuedFilesInGuildAsync(IGuild guild, IAudioClient client) {
            _playingInGuild[guild.Id] = true;
            List<string> playQueue = _queue[guild.Id];
            int length = playQueue.Count;
            int oldLength = length;
            Logging.Debug($"Playing {length} queued file(s)");
            for (int i = 0; i < length; i++) {
                await SendAudioAsync(client, playQueue[i]);
                length = _queue[guild.Id].Count;
                if (length != oldLength) {
                    Logging.Debug($"Queue length has changed! Old value: {oldLength}, New Value: {length}");
                    playQueue = _queue[guild.Id];
                    oldLength = length;
                }
            }
            Logging.Debug("Reached the end of the queue! Clearing it...");
            _queue[guild.Id].Clear();
            _playingInGuild[guild.Id] = false;
            await Task.CompletedTask;
        }

        public static async Task SendAudioAsync(IAudioClient client, string path) {
            Logging.Debug($"Sending audio from {path}");
            using (var ffmpeg = CreateStream(path))
            using (var output = ffmpeg.StandardOutput.BaseStream)
            using (var discord = client.CreatePCMStream(AudioApplication.Mixed)) {
                try {
                    Logging.Debug("Sending...");
                    await output.CopyToAsync(discord);
                } finally {
                    Logging.Debug("Finished sending audio.");
                    await discord.FlushAsync();
                }
            }
        }

    }
}
