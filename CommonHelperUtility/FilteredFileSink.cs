using Serilog.Core;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace CommonHelperUtility
{
    public class FilteredFileSink : ILogEventSink
    {
        private readonly string _logFilePath;
        private readonly object _lock = new object();
        private readonly JsonFormatter _formatter = new JsonFormatter();

        public FilteredFileSink(string logFilePath)
        {
            _logFilePath = logFilePath;
        }
        public void Emit(LogEvent logEvent)
        {
            if (!IsSuppressedLog(logEvent.MessageTemplate.Text))
            {
                lock (_lock)
                {
                    using (var writer = File.AppendText(_logFilePath))
                    {
                        _formatter.Format(logEvent, writer);
                    }
                }
            }
        }
        private static bool IsSuppressedLog(string message)
        {
            return message.Contains("Request starting HTTP/2 GET") ||
                   message.Contains("Request finished HTTP/2 GET") ||
                   message.Contains("User profile is available.") ||
                   message.Contains("Now listening on:") ||
                   message.Contains("Application started.") ||
                   message.Contains("Hosting environment:") ||
                   message.Contains("Content root path:");
        }
    }
}