using Microsoft.Data.SqlClient;  // << required

namespace EmployeeCrudPdf.Data
{
    public interface ISqlConnectionFactory
    {
        SqlConnection Create();
    }
}
