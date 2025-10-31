namespace EmployeeCrudPdf.Services.Logging
{
    public interface IAppLogger
    {
        void Info(string source, string message);
        void Warn(string source, string message);
        void Error(string source, string message, Exception? ex = null);
    }
}
