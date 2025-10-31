using Microsoft.Data.SqlClient;            // << required
using Microsoft.Extensions.Configuration;

namespace EmployeeCrudPdf.Data
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _conn;

        public SqlConnectionFactory(IConfiguration cfg)
        {
            _conn = cfg.GetConnectionString("SqlServer")!;
        }

        public SqlConnection Create() => new(_conn);  // matches interface
    }
}
