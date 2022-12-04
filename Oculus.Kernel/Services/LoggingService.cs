using System.Text;
using Microsoft.Extensions.Logging;
using Oculus.Common.Utilities.Extensions;
using Pastel;

namespace Oculus.Kernel.Services
{
    public interface ILoggingService
    {
        void Info(string message, Exception? exception = null, string? className = null);
        void Debug(string message, Exception? exception = null, string? className = null);
        void Verbose(string message, Exception? exception = null, string? className = null);
        void Warn(string message, Exception? exception = null, string? className = null);
        void Error(string message, Exception? exception = null, string? className = null);
        void Critical(string message, Exception? exception = null, string? className = null);

        void Log(object source, string message, LogLevel level = LogLevel.Information, Exception exception = null);
    }

    public class LoggingService : ILogger, ILoggingService
    {
        private readonly object _lock = new object();

        private readonly string _name = nameof(Oculus).ToLowerInvariant();

        public void Info(string message, Exception? exception = null,
            string? className = null) =>
            BaseLog("info:", "#00ddff", message, exception, className);

        public void Debug(string message, Exception? exception = null,
            string? className = null)
        {
#if DEBUG
            BaseLog("debug:", "#8a2be2", message, exception, className);
#endif
        }

        public void Verbose(string message, Exception? exception = null,
            string? className = null) =>
            BaseLog("verb:", "#00ff33", message, exception, className);

        public void Warn(string message, Exception? exception = null,
            string? className = null) =>
            BaseLog("warn:", "#ffa500", message, exception, className);

        public void Error(string message, Exception? exception = null,
            string? className = null) =>
            BaseLog("error:", "#ff3333", message, exception, className);

        public void Critical(string message, Exception? exception = null,
            string? className = null) =>
            BaseLog("crit:", "#ff0000", message, exception, className);

        public void Log(object source, string message, LogLevel level = LogLevel.Information, Exception exception = null)
        {
            switch (level)
            {
                case LogLevel.Information:
                    Info(message, exception, source.GetType().Name);
                    break;

                case LogLevel.Warning:
                    Warn(message, exception, source.GetType().Name);
                    break;

                case LogLevel.Trace:
                    Verbose(message, exception, source.GetType().Name);
                    break;

                case LogLevel.Critical:
                    Critical(message, exception, source.GetType().Name);
                    break;

                case LogLevel.Debug:
                    Debug(message, exception, source.GetType().Name);
                    break;

                case LogLevel.Error:
                    Error(message, exception, source.GetType().Name);
                    break;
            }
        }

        private void BaseLog(string name, string color,
            string message, Exception? exception = null, string? className = null)
        {
            if (string.IsNullOrEmpty(message))
                return;

            string padded = (className ?? _name).Truncate(8, false).PadRight(_name.Length + 3);
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

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            throw new NotImplementedException();
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            throw new NotImplementedException();
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        {
            throw new NotImplementedException();
        }
    }
}
