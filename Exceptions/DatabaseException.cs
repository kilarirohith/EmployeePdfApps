namespace EmployeeCrudPdf.Exceptions
{
    public class DatabaseException : AppException
    {
        public DatabaseException(string message, Exception inner) : base(message, inner) { }
    }
}
