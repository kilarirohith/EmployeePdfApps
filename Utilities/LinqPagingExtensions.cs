namespace EmployeeCrudPdf.Utilities
{
    public static class LinqPagingExtensions
    {
        public static IEnumerable<T> Page<T>(this IEnumerable<T> src, int page, int pageSize)
            => src.Skip((page - 1) * pageSize).Take(pageSize);
    }
}
