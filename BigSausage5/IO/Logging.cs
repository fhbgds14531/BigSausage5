using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Discord;

namespace BigSausage {
	public class Logging {

		private static readonly LogSeverity severityThreshold = LogSeverity.Debug;
		private static readonly string LogFileName = DateTime.Now.ToString().Replace("/", ".").Replace(":", ".") + " Log.txt";
		private static readonly string LogPath = Utils.GetProcessPathDir() + "\\Files\\Logging";

		public static void Debug(string message, [CallerMemberName] string memberName = "@", [CallerFilePath] string callerPath = "@") {
			Log(new LogMessage(LogSeverity.Debug, callerPath.Substring(callerPath.LastIndexOf(@"\") + 1).Replace(".cs", ""), $"[{memberName}] {message}"));
		}

		public static void Info(string message, [CallerMemberName] string memberName = "@", [CallerFilePath] string callerPath = "@") {
			Log(new LogMessage(LogSeverity.Info, callerPath.Substring(callerPath.LastIndexOf(@"\") + 1).Replace(".cs", ""), $"[{memberName}] {message}"));
		}

		public static void Verbose(string message, [CallerMemberName] string memberName = "@", [CallerFilePath] string callerPath = "@") {
			Log(new LogMessage(LogSeverity.Verbose, callerPath.Substring(callerPath.LastIndexOf(@"\") + 1).Replace(".cs", ""), $"[{memberName}] {message}"));
		}

		public static void Warning(string message, [CallerMemberName] string memberName = "@", [CallerFilePath] string callerPath = "@") {
			Log(new LogMessage(LogSeverity.Warning, callerPath.Substring(callerPath.LastIndexOf(@"\") + 1).Replace(".cs", ""), $"[{memberName}] {message}"));
		}

		public static void Critical(string message, [CallerMemberName] string memberName = "@", [CallerFilePath] string callerPath = "@") {
			Log(new LogMessage(LogSeverity.Critical, callerPath.Substring(callerPath.LastIndexOf(@"\") + 1).Replace(".cs", ""), $"[{memberName}] {message}"));
		}

		public static void Error(string message, [CallerMemberName] string memberName = "@", [CallerFilePath] string callerPath = "@") {
			Log(new LogMessage(LogSeverity.Error, callerPath.Substring(callerPath.LastIndexOf(@"\") + 1).Replace(".cs", ""), $"[{memberName}] {message}"));
		}

		public static Task Log(LogMessage msg) {
			String message = msg.ToString();
			LogSeverity severity = msg.Severity;
			switch (severity) {
				default:
					case LogSeverity.Debug:
					if (severityThreshold >= LogSeverity.Debug) {
						Console.ForegroundColor = ConsoleColor.Gray;
						message = "[Debug]    " + message;
						break;
					} else {
						return Task.CompletedTask;
					}
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
			Critical(description);
			Critical("Exception " + e.GetType().FullName + " occured!");
			Critical(e.Message);
			if (e.TargetSite != null) {
				Critical("Occured at " + e.TargetSite.Name + " in " + e.TargetSite.Module.FullyQualifiedName);
			}
		}

		public async static Task<Task> LogErrorToFileAsync(IGuild? guild, IMessage? triggerMessage, string summary) {
			Console.Out.WriteLine("Testing Logging...");
			List<string> lines = new();

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

			await errorFile.WriteAsync(buffer);
			errorFile.Close();

			return Task.CompletedTask;
		}

		public static void LogErrorToFile(IGuild? guild, IMessage? triggerMessage, string summary) {
			List<string> lines = new();

			lines.Add("An error occured" + (guild == null ? "!" : " in guild \"" + guild.Name + "\" (" + guild.Id + ")"));
			lines.Add("\t\"Trigger message: " + (triggerMessage == null ? "[NO_MESSAGE]" : triggerMessage.Content) + "\"");
			lines.Add("\nSummary: " + summary);
			lines.Add("\n[Stacktrace]");

			lines.Add(Environment.StackTrace);

			string? loggingFolder = Utils.GetProcessPathDir();

			if (loggingFolder == null) {
				Error("Error logging to file: Process Path (" + loggingFolder + ") is null.");
				return;
			}

			loggingFolder = loggingFolder.Replace("\\BigSausage5.exe", "");
			loggingFolder += "\\Files\\Logging\\Error Files";

			string errorFilePath = loggingFolder + "\\" + LogFileName.Replace(" Log.txt", "");

			IO.IOUtilities.AssertDirectoryExists(errorFilePath);

			FileStream errorFile = File.Create(errorFilePath + "\\ERROR_" + System.DateTime.Now.ToString().Replace("/", "_").Replace(":", ".") + ".txt");

			string outputString = lines.Aggregate((x, y) => x + "\n" + y);

			byte[] buffer = Encoding.UTF8.GetBytes(outputString);

			errorFile.Write(buffer, 0, buffer.Length);
			errorFile.Close();

			return;
		}

	}
}
