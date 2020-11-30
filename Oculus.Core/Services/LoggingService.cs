using Discord;
using Pastel;
using Oculus.Common.Structures;
using System;
using System.Text;

namespace Oculus.Core.Services
{
	public interface ILoggingService
	{
		void Log(LogSeverity severity, string message, Exception exception = null, string className = null);

		void Info(string message, Exception exception = null, string className = null);
		void Debug(string message, Exception exception = null, string className = null);
		void Verbose(string message, Exception exception = null, string className = null);
		void Warn(string message, Exception exception = null, string className = null);
		void Error(string message, Exception exception = null, string className = null);
		void Critical(string message, Exception exception = null, string className = null);
	}

	public class LoggingService : ServiceBase, ILoggingService
	{
		private readonly object _lock = new object();

		private string _name = "oculus";

		public void Log(LogSeverity severity, string message,
			Exception exception = null, string className = null)
		{
			switch (severity)
			{
				case LogSeverity.Info:
					Info(message, exception, className);
					break;

				case LogSeverity.Debug:
					Debug(message, exception, className);
					break;

				case LogSeverity.Verbose:
					Verbose(message, exception, className);
					break;

				case LogSeverity.Warning:
					Warn(message, exception, className);
					break;

				case LogSeverity.Error:
					Error(message, exception, className);
					break;

				case LogSeverity.Critical:
					Critical(message, exception, className);
					break;

				default:
					Info(message, exception, className);
					break;
			}
		}

		public void Info(string message, Exception exception = null,
			string className = null) =>
			BaseLog("info:", "#00ddff", message, exception, className);

		public void Debug(string message, Exception exception = null,
			string className = null)
		{
#if DEBUG
			BaseLog("debug:", "#8a2be2", message, exception, className);
#endif
		}

		public void Verbose(string message, Exception exception = null,
			string className = null) =>
			BaseLog("verb:", "#00ff33", message, exception, className);

		public void Warn(string message, Exception exception = null,
			string className = null) =>
			BaseLog("warn:", "#ffa500", message, exception, className);

		public void Error(string message, Exception exception = null,
			string className = null) =>
			BaseLog("error:", "#ff3333", message, exception, className);

		public void Critical(string message, Exception exception = null,
			string className = null) =>
			BaseLog("crit:", "#ff0000", message, exception, className);


		private void BaseLog(string name, string color,
			string message, Exception exception = null, string className = null)
		{
			if (string.IsNullOrEmpty(message))
				return;

			string padded = (className ?? _name).PadRight(_name.Length);
			var output = new StringBuilder();
			output.Append($"[{DateTime.Now.ToLongTimeString()}] ".Pastel("#666666"));
			output.Append($"{padded.Pastel("#888888")} ");
			output.Append($"{name.PadLeft(6).Pastel(color)} ");
			output.Append(message.Pastel("#cfcfcf"));

			if (exception is not null)
				output.Append($"\n{exception.Message}");

			lock (_lock)
			{
				Console.WriteLine(output);
			}
		}
	}
}
