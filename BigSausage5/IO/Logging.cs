using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace BigSausage {
	public class Logging {

		private static LogSeverity severityThreshold = LogSeverity.Debug;
		private static string LogFileName = DateTime.Now.ToString().Replace("/", ".").Replace(":", ".") + " Log.txt";
		private static string LogPath = Utils.GetProcessPathDir() + "\\Files\\Logging";

		[Obsolete("Please use the version that includes severity.")]
		public static void Log(string message) {
			Log(new LogMessage(LogSeverity.Info, "BigSausage", message));
		}

		public static void Log(string message, LogSeverity severity) {
			Log(new LogMessage(severity, "BigSausage", message));
		}

		public static Task Log(LogMessage msg) {
			String message = msg.ToString();
			LogSeverity severity = msg.Severity;
			switch (severity) {
				default:
				case LogSeverity.Verbose:
					if (severityThreshold >= LogSeverity.Verbose) {
						Console.ForegroundColor = ConsoleColor.Gray;
						message = "[Verbose]  " + message;
						break;
					} else {
						return Task.CompletedTask;
					}
				case LogSeverity.Info:
					if (severityThreshold >= LogSeverity.Info) {
						Console.ForegroundColor = ConsoleColor.White;
						message = "[Info]     " + message;
						break;
					} else {
						return Task.CompletedTask;
					}
				case LogSeverity.Warning:
					if (severityThreshold >= LogSeverity.Warning) {
						Console.ForegroundColor = ConsoleColor.Yellow;
						message = "[Warning]  " + message;
						break;
					} else {
						return Task.CompletedTask;
					}
				case LogSeverity.Error:
					if (severityThreshold >= LogSeverity.Error) {
						Console.ForegroundColor = ConsoleColor.Red;
						message = "[Error]    " + message;
						break;
					} else {
						return Task.CompletedTask;
					}
				case LogSeverity.Critical:
					if (severityThreshold >= LogSeverity.Critical) {
						Console.ForegroundColor = ConsoleColor.DarkRed;
						message = "[Critical] " + message;
						break;
					} else {
						return Task.CompletedTask;
					}
			}
			Console.WriteLine(message);
			IO.IOUtilities.WriteLineToFile(message, LogPath, LogFileName);
			Console.ForegroundColor = ConsoleColor.White;
			return Task.CompletedTask;
		}

		public static void LogException(Exception e, string description) {
			LogErrorToFile(null, null, description + " (" + e.GetType().FullName + ")");
			Log("Exception " + e.GetType().FullName + " occured!", LogSeverity.Critical);
			Log(e.Message, LogSeverity.Critical);
			Log(description, LogSeverity.Critical);
			if (e.TargetSite != null) {
				Log("Occured at " + e.TargetSite.Name + " in " + e.TargetSite.Module.FullyQualifiedName, LogSeverity.Critical);
			}
		}

		public async static Task<Task> LogErrorToFileAsync(IGuild? guild, IMessage? triggerMessage, string summary) {
			Console.Out.WriteLine("Testing Logging...");
			List<string> lines = new List<string>();

			lines.Add("An error occured" + (guild == null ? "!" : " in guild \"" + guild.Name + "\" (" + guild.Id + ")"));
			lines.Add("\t\"Trigger message: " + (triggerMessage == null ? "[NO_MESSAGE]" : triggerMessage.Content) + "\"");
			lines.Add("\nSummary: " + summary);
			lines.Add("\n[Stacktrace]");

			lines.Add(Environment.StackTrace);

			//string? desktopPath = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);
			string? loggingFolder = Environment.ProcessPath;

			if (loggingFolder == null) {
				Console.WriteLine("Error logging to file: Process Path (" + loggingFolder + ") is null.");
				return Task.CompletedTask;
			}

			loggingFolder = loggingFolder.Replace("\\BigSausage5.exe", "");
			loggingFolder += "\\Files\\Logging\\Error Files";

			string errorFilePath = loggingFolder + "\\" + LogFileName.Replace(" Log.txt", "");
			IO.IOUtilities.AssertDirectoryExists(errorFilePath);

			FileStream errorFile = File.Create(loggingFolder + "\\ERROR_" + System.DateTime.Now.ToString().Replace("/", "_").Replace(":", ".") + ".txt");

			string outputString = lines.Aggregate((x, y) => x + "\n" + y);

			byte[] buffer = Encoding.UTF8.GetBytes(outputString);

			await errorFile.WriteAsync(buffer, 0, buffer.Length);
			errorFile.Close();

			return Task.CompletedTask;
		}

		public static void LogErrorToFile(IGuild? guild, IMessage? triggerMessage, string summary) {
			List<string> lines = new List<string>();

			lines.Add("An error occured" + (guild == null ? "!" : " in guild \"" + guild.Name + "\" (" + guild.Id + ")"));
			lines.Add("\t\"Trigger message: " + (triggerMessage == null ? "[NO_MESSAGE]" : triggerMessage.Content) + "\"");
			lines.Add("\nSummary: " + summary);
			lines.Add("\n[Stacktrace]");

			lines.Add(Environment.StackTrace);

			string? loggingFolder = Utils.GetProcessPathDir();

			if (loggingFolder == null) {
				Console.WriteLine("Error logging to file: Process Path (" + loggingFolder + ") is null.");
				return;
			}

			loggingFolder = loggingFolder.Replace("\\BigSausage5.exe", "");
			loggingFolder += "\\Files\\Logging\\Error Files";

			string errorFilePath = loggingFolder + "\\" + LogFileName.Replace(" Log.txt", "");

			IO.IOUtilities.AssertDirectoryExists(errorFilePath);

			FileStream errorFile = File.Create(loggingFolder + "\\ERROR_" + System.DateTime.Now.ToString().Replace("/", "_").Replace(":", ".") + ".txt");

			string outputString = lines.Aggregate((x, y) => x + "\n" + y);

			byte[] buffer = Encoding.UTF8.GetBytes(outputString);

			errorFile.Write(buffer, 0, buffer.Length);
			errorFile.Close();

			return;
		}

	}
}
