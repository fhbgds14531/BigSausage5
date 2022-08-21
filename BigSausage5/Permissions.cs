using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace BigSausage.Permissions {

	public enum EnumPermissionLevel {
		None = 0,
		Low = 1,
		Medium = 2,
		High = 3,
		Admin = 4,
		ServerOwner = 5,
		BotCreator = int.MaxValue
	} 

	public class Permissions {

		internal static readonly ulong ME = 198575970624471040;

		private static bool _initialized = false;

		private static Dictionary<ulong, Dictionary<ulong, int>> _loadedPermissions;

		public Permissions() {
			_loadedPermissions = new();
		}

		public static void Initialize() {
			Logging.Info("Initializing permissions...");
			if (!_initialized) {
				DiscordSocketClient? client = BigSausage.GetClient();
				if (client != null) {
					_loadedPermissions = new Dictionary<ulong, Dictionary<ulong, int>>();
					Logging.Debug("Loading permissions from disk...");
					foreach (KeyValuePair<ulong, SerializableDictionary<ulong, int>> pair in IO.IOUtilities.GetPermissionLevelsFromDisk()) { //Guild
						Logging.Debug($"Processing permissions for guild {pair.Key}...");
						Dictionary<ulong, int> userPerms = new();
						foreach (KeyValuePair<ulong, int> user in pair.Value) { //User
							userPerms[user.Key] = user.Value;
						}
						_loadedPermissions[pair.Key] = userPerms;
					}
					Logging.Debug("Permissions Initialized successfully!");
					_initialized = true;
					foreach (IGuild guild in client.Guilds) {
						InitPermissionsForGuild(guild);
					}
					return;
				} else {
					Logging.Error("Client is null during permissions initialization! Permissions cannot be initialized before the client exists.");
					Logging.LogErrorToFile(null, null, "Client is null during permissions initialization! Permissions cannot be initialized before the client exists.");
					throw new InvalidOperationException("Attempted to initialize permissions before the client existed!");
				}
			} else {
				Logging.Warning("Attempted to initialize permissions more than once! Skipping...");
				Logging.LogErrorToFile(null, null, "Attempted to initialize Permissions more than once!");
				return;
			}
		}

		public static void InitPermissionsForGuild(IGuild guild) {
			if (!_initialized) {
				Logging.Warning("Permissions interaction was requested but has not yet been initialized. Initializing...");
				Initialize();
			}
			Logging.Warning("Beginning initialization of permissions for guild " + guild.Name + " (" + guild.Id + ")...");
			IReadOnlyCollection<IGuildUser> users = guild.GetUsersAsync().Result;
			Dictionary<ulong, int> perms;
			if (_loadedPermissions.ContainsKey(guild.Id)) {
				perms = _loadedPermissions[guild.Id];
				Logging.Verbose("Permissions for guild " + guild.Name + " (" + guild.Id + ") loaded successfully!");
			} else {
				Logging.Verbose("Permissions for guild " + guild.Name + " (" + guild.Id + ") couldn't be found! Setting default permissions...");
				perms = new Dictionary<ulong, int>();
			}
			foreach (IGuildUser user in users) {
				if (!perms.ContainsKey(user.Id)) {
					if (user.IsBot) {
						perms[user.Id] = (int) EnumPermissionLevel.None;
						Logging.Verbose("User " + user.Username + " (" + user.Id + ") is a bot! Defaulting to None...");
					} else if (user.Id == ME) {
						perms[user.Id] = (int)EnumPermissionLevel.BotCreator;
						Logging.Verbose("User " + user.Username + " (" + user.Id + ") is not present in permissions! User is bot creator, defaulting to BotCreator...");
					} else if (guild.GetOwnerAsync().Result.Id == user.Id) {
						perms[user.Id] = (int)EnumPermissionLevel.ServerOwner;
						Logging.Verbose("User " + user.Username + " (" + user.Id + ") is not present in permissions! User is server owner, defaulting to ServerOwner...");
					} else if (user.GuildPermissions.Administrator) {
						perms[user.Id] = (int)EnumPermissionLevel.Admin;
						Logging.Verbose("User " + user.Username + " (" + user.Id + ") is not present in permissions! User is an administrator, defaulting to Admin...");
					} else {
						perms[user.Id] = (int)EnumPermissionLevel.Medium;
						Logging.Verbose("User " + user.Username + " (" + user.Id + ") is not present in permissions! Defaulting to Medium...");
					}
				} else {
					Logging.Verbose("User " + user.Username + " (" + user.Id + ") is present in permissions, skipping...");
				}
			}
			_loadedPermissions[guild.Id] = perms;
			Logging.Verbose("Initialized permissions!");
		}

		public static EnumPermissionLevel GetUserPermissionLevelInGuild(IGuild guild, IUser user) {
			if (!_initialized) {
				Logging.Warning("Permission level has been requested but permissions have not been initialized! Initializing...");
				Initialize();
			}
			return (EnumPermissionLevel) _loadedPermissions[guild.Id][user.Id];
		}

		public static bool UserMeetsPermissionRequirements(SocketCommandContext commandContext, EnumPermissionLevel permissionLevel) {
			return UserMeetsPermissionRequirements(commandContext.Guild, commandContext.User, permissionLevel);
		}

		public static bool UserMeetsPermissionRequirements(IGuild guild, IUser user, EnumPermissionLevel permissionLevel) {
			if (!_initialized) {
				Logging.Warning("Permission level has been requested but permissions have not been initialized! Initializing...");
				Initialize();
			}
			return user.Id == ME || (int) permissionLevel <= _loadedPermissions[guild.Id][user.Id];
		}

		public static void Save() {
			if (!_initialized) {
				Logging.Warning("Permissions save was requested but it has not been initialized! Ignoring request...");
				return;
			}
			Logging.Debug("Converting permissions to serializable data...");
			SerializableDictionary<ulong, SerializableDictionary<ulong, int>> output = new();
			foreach (KeyValuePair<ulong, Dictionary<ulong, int>> pair in _loadedPermissions) { //Guild
				SerializableDictionary<ulong, int> userPerms = new();
				foreach (KeyValuePair<ulong, int> user in pair.Value) { //User
					userPerms[user.Key] = user.Value;
				}
				output[pair.Key] = userPerms;
			}
			Logging.Debug("Done!");
			Logging.Verbose("Saving permissions to disk...");
			IO.IOUtilities.SavePermissionsToDisk(output);
		}

		public static void Reload() {
			Logging.Debug("Reloading permissions...");
			_loadedPermissions = new();
			_initialized = false;
			Initialize();
		}
	}
}
