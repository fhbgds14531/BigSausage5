using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BigSausage.IO {
	public class FileLogger {


		Dictionary<string, List<string>> _queue;
		Dictionary<string, List<string>> _writeQueue;

		public FileLogger() {
			Console.WriteLine("Initializing FileLogger...");
			this._queue = new();
			this._writeQueue = new();
			Thread t = new Thread(Run);
			t.Name = "Text file line writing batcher";
			t.Start();
		}

		private async void Run() {
			while (!BigSausage.GetBotMainProcess().HasExited) {
				await WriteQueuedLinesToFile();
				await Task.Delay(500);
			}
		}

		public void AddLineToQueue(string filePath, string line) {
			if(!_queue.ContainsKey(filePath)) _queue.Add(filePath, new());
			_queue[filePath].Add(line);
		}

		int MaxRetries = 10;
		int DelayOnRetry = 25;
		private async Task WriteQueuedLinesToFile() {
			_writeQueue = _queue;
			_queue = new();
			foreach (KeyValuePair<string, List<string>> pair in _writeQueue) {
				string file = pair.Key;
				List<string> lines = pair.Value;
				for (int i = 1; i <= MaxRetries; i++) {
					try {
						if (lines.Count > 0) {
							await File.AppendAllLinesAsync(file, lines);
						}
					} catch (Exception ex) when (i >= MaxRetries) {
						Console.WriteLine(ex.Message);
						await Task.Delay(DelayOnRetry);
					}
				}
			}
		}

	}
}
