using Discord;
using Pastel;
using System;
using System.Text;

namespace Spade.Core.Services
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

		public void Log(LogSeverity severity, string message,
			Exception exception = null, string className = "spade")
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
			BaseLog("verbose:", "#00ff33", message, exception, className);

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
			var output = new StringBuilder();

			if (message is null || message.Length < 1)
				return;

			if (className is not null)
				output = new StringBuilder($"{className.Pastel("#888888")} ");

			output.Append($"{name.Pastel(color)} ");
			output.Append(message?.Pastel("#cfcfcf"));

			if (exception is not null)
				output.Append($"\n{exception.Message}");

			lock (_lock)
			{
				Console.WriteLine(output);
			}
		}

		public static LoggingService operator |(string text, LoggingService loggingService)
		{
			loggingService.Info(text);
			return loggingService;
		}

		public static LoggingService operator |(ValueTuple<string, LogSeverity> tuple, LoggingService loggingService)
		{
			var (text, logSeverity) = tuple;
			loggingService.Log(logSeverity, text);
			return loggingService;
		}
	}
}
