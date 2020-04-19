using System;
using Discord;
using Pastel;

namespace Muon.Services
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

    public class LoggingService : ILoggingService
    {
        private readonly object _lock = new object();

        public LoggingService()
        { }

        public void Log(LogSeverity severity, string message,
            Exception exception = null, string className = "muon")
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
            string className = null) =>
            BaseLog("debug:", "#00dd44", message, exception, className);

        public void Verbose(string message, Exception exception = null,
            string className = null) =>
            BaseLog("verbose:", "#00ff33", message, exception, className);

        public void Warn(string message, Exception exception = null,
            string className = null) =>
            BaseLog("warn:", "#ffa500", message, exception, className);

        public void Error(string message, Exception exception = null,
            string className = null) =>
            BaseLog("info:", "#ff3333", message, exception, className);

        public void Critical(string message, Exception exception = null,
            string className = null) =>
            BaseLog("info:", "#ff0000", message, exception, className);


        private void BaseLog(string name, string color,
            string message, Exception exception = null, string className = null)
        {
            var output = "";

            if (!(className is null))
                output = $"{className.Pastel("#888888")} ";

            output += $"{name.Pastel(color)} ";
            output += message.Pastel("#cfcfcf");
            output += exception is null ? "" : $"\n{exception.Message}";

            lock (_lock)
            {
                Console.WriteLine(output);
            }
        }
    }
}
