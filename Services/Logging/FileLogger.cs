using System.Text;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;

namespace EmployeeCrudPdf.Services.Logging
{
    public sealed class FileLogger : IAppLogger
    {
        private readonly string _logDir;
        private readonly object _sync = new();

        public FileLogger(IWebHostEnvironment env, IConfiguration cfg)
        {
            var configured = cfg["LoggingFile:Directory"];
            _logDir = string.IsNullOrWhiteSpace(configured)
                ? Path.Combine(env.ContentRootPath, "Logs")
                : Path.IsPathRooted(configured) ? configured : Path.Combine(env.ContentRootPath, configured);
            Directory.CreateDirectory(_logDir);
        }

        private string CurrentFile() => Path.Combine(_logDir, $"logs_{DateTime.UtcNow:yyyy-MM-dd}.txt");

        private static string Compose(string level, string source, string message, Exception? ex)
        {
            var sb = new StringBuilder();
            sb.AppendLine($"[{DateTime.UtcNow:yyyy-MM-dd HH:mm:ss} UTC] [{level}] [{source}]");
            sb.AppendLine($"Message: {message}");
            if (ex != null)
            {
                sb.AppendLine($"Exception: {ex.GetType().Name}: {ex.Message}");
                sb.AppendLine($"StackTrace: {ex.StackTrace}");
                if (ex.InnerException != null)
                    sb.AppendLine($"Inner: {ex.InnerException.GetType().Name}: {ex.InnerException.Message}");
            }
            sb.AppendLine(new string('-', 70));
            return sb.ToString();
        }

        private void Write(string payload)
        {
            try { lock (_sync) File.AppendAllText(CurrentFile(), payload); }
            catch { /* swallow logging errors */ }
        }

        public void Info(string source, string message) => Write(Compose("INFO", source, message, null));
        public void Warn(string source, string message) => Write(Compose("WARNING", source, message, null));
        public void Error(string source, string message, Exception? ex = null) => Write(Compose("ERROR", source, message, ex));
    }
}