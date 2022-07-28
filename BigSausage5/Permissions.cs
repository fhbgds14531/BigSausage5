using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;
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

		private static bool initialized = false;

		private static Dictionary<ulong, Dictionary<ulong, int>> _loadedPermissions;

		private static void Initialize() {
			Logging.Log("Initializing permissions...", LogSeverity.Info);
			if (!initialized) {
				DiscordSocketClient client = BigSausage.GetClient();
				if (client != null) {
					_loadedPermissions = new Dictionary<ulong, Dictionary<ulong, int>>();
					foreach (KeyValuePair<ulong, SerializableDictionary<ulong, int>> pair in IO.IOUtilities.GetPermissionLevelsFromDisk()) { //Guild
						Dictionary<ulong, int> userPerms = new Dictionary<ulong, int>();
						foreach (KeyValuePair<ulong, int> user in pair.Value) { //User
							userPerms[user.Key] = user.Value;
						}
						_loadedPermissions[pair.Key] = userPerms;
					}
					initialized = true;
				} else {
					Logging.Log("Client is null during permissions initialization! Permissions cannot be initialized before the client exists.", LogSeverity.Error);
					Logging.LogErrorToFile(null, null, "Client is null during permissions initialization! Permissions cannot be initialized before the client exists.");
					throw new InvalidOperationException("Attempted to initialize permissions before the client existed!");
				}
			} else {
				Logging.Log("Attempted to initialize permissions more than once! Skipping...", LogSeverity.Warning);
				Logging.LogErrorToFile(null, null, "Attempted to initialize Permissions more than once!");
				return;
			}
		}

		public static void InitPermissionsForGuild(IGuild guild) {
			if (!initialized) {
				Logging.Log("Permissions interaction was requested but has not yet been initialized. Initializing...", LogSeverity.Warning);
				Initialize();
			}
			Logging.Log("Beginning initialization of permissions for guild " + guild.Name + " (" + guild.Id + ")...", LogSeverity.Verbose);
			IReadOnlyCollection<IGuildUser> users = guild.GetUsersAsync().Result;
			Dictionary<ulong, int> perms;
			if (_loadedPermissions.ContainsKey(guild.Id)) {
				perms = _loadedPermissions[guild.Id];
				Logging.Log("Permissions for guild " + guild.Name + " (" + guild.Id + ") loaded successfully!", LogSeverity.Verbose);
			} else {
				Logging.Log("Permissions for guild " + guild.Name + " (" + guild.Id + ") couldn't be found! Setting default permissions...", LogSeverity.Verbose);
				perms = new Dictionary<ulong, int>();
			}
			foreach (IGuildUser user in users) {
				if (!perms.ContainsKey(user.Id)) {
					if (user.IsBot) {
						perms[user.Id] = (int) EnumPermissionLevel.None;
						Logging.Log("User " + user.Username + " (" + user.Id + ") is a bot! Defaulting to None...", LogSeverity.Verbose);
					} else if (user.Id == ME) {
						perms[user.Id] = (int)EnumPermissionLevel.BotCreator;
						Logging.Log("User " + user.Username + " (" + user.Id + ") is not present in permissions! User is bot creator, defaulting to BotCreator...", LogSeverity.Verbose);
					} else if (guild.GetOwnerAsync().Result.Id == user.Id) {
						perms[user.Id] = (int)EnumPermissionLevel.ServerOwner;
						Logging.Log("User " + user.Username + " (" + user.Id + ") is not present in permissions! User is server owner, defaulting to ServerOwner...", LogSeverity.Verbose);
					} else if (user.GuildPermissions.Administrator) {
						perms[user.Id] = (int)EnumPermissionLevel.Admin;
						Logging.Log("User " + user.Username + " (" + user.Id + ") is not present in permissions! User is an administrator, defaulting to Admin...", LogSeverity.Verbose);
					} else {
						perms[user.Id] = (int)EnumPermissionLevel.Medium;
						Logging.Log("User " + user.Username + " (" + user.Id + ") is not present in permissions! Defaulting to Medium...", LogSeverity.Verbose);
					}
				} else {
					Logging.Log("User " + user.Username + " (" + user.Id + ") is present in permissions, skipping...", LogSeverity.Verbose);
				}
			}
			_loadedPermissions[guild.Id] = perms;
			Logging.Log("Initialized permissions!", LogSeverity.Verbose);
		}

		public static EnumPermissionLevel GetUserPermissionLevelInGuild(IGuild guild, IUser user) {
			if (!initialized) {
				Logging.Log("Permission level has been requested but permissions have not been initialized! Initializing...", LogSeverity.Warning);
				Initialize();
			}
			return (EnumPermissionLevel) _loadedPermissions[guild.Id][user.Id];
		}

		public static bool UserMeetsPermissionRequirements(IGuild guild, IUser user, EnumPermissionLevel permissionLevel) {
			if (!initialized) {
				Logging.Log("Permission level has been requested but permissions have not been initialized! Initializing...", LogSeverity.Warning);
				Initialize();
			}
			return user.Id == ME ? true : (int) permissionLevel <= _loadedPermissions[guild.Id][user.Id];
		}

		public static void Save() {
			if (!initialized) {
				Logging.Log("Permissions save was requested but it has not been initialized! Ignoring request...", LogSeverity.Warning);
				return;
			}
			Logging.Log("Converting permissions to serializable data...", LogSeverity.Debug);
			SerializableDictionary<ulong, SerializableDictionary<ulong, int>> output = new SerializableDictionary<ulong, SerializableDictionary<ulong, int>>();
			foreach (KeyValuePair<ulong, Dictionary<ulong, int>> pair in _loadedPermissions) { //Guild
				SerializableDictionary<ulong, int> userPerms = new SerializableDictionary<ulong, int>();
				foreach (KeyValuePair<ulong, int> user in pair.Value) { //User
					userPerms[user.Key] = user.Value;
				}
				output[pair.Key] = userPerms;
			}
			Logging.Log("Done!", LogSeverity.Debug);
			Logging.Log("Saving permissions to disk...", LogSeverity.Verbose);
			IO.IOUtilities.SavePermissionsToDisk(output);
		}
	}
}
