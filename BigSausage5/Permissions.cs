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
					_loadedPermissions = IO.IOUtilities.GetPermissionLevelsFromDisk();
					initialized = true;
				} else {
					Logging.Log("[ERROR] Client is null during permissions initialization! Permissions cannot be initialized before the client exists.", LogSeverity.Error);
					Logging.LogErrorToFile(null, null, "Client is null during permissions initialization! Permissions cannot be initialized before the client exists.");
					throw new InvalidOperationException("Attempted to initialize permissions before the client existed!");
				}
			} else {
				Logging.Log("Attempted to initialize permissions more than once! Skipping...", LogSeverity.Warning);
				Logging.LogErrorToFile(null, null, "Attempted to initialize Permissions more than once!");
				return;
			}
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
	}
}
