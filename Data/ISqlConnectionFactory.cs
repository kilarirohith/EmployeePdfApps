using Microsoft.Data.SqlClient;  

namespace EmployeeCrudPdf.Data
{
    public interface ISqlConnectionFactory
    {
        SqlConnection Create();
    }
}
