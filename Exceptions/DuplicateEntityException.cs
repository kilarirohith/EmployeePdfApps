namespace EmployeeCrudPdf.Exceptions
{
    public class DuplicateEntityException : AppException
    {
        public DuplicateEntityException(string entity, string field, object value)
            : base($"{entity} with {field} '{value}' already exists.") { }
    }
}
