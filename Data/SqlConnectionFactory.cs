using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using EmployeeCrudPdf.Services.Logging;

namespace EmployeeCrudPdf.Data
{
    public class SqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _conn;

        public SqlConnectionFactory(IConfiguration cfg, IAppLogger log)
        {
            _conn = cfg.GetConnectionString("SqlServer")
                ?? throw new InvalidOperationException("Missing ConnectionStrings:DefaultConnection in configuration.");
            
            // (Optional) Log a sanitized version once, to prove it's loaded
            var safe = _conn.Replace("Password=", "Password=***", StringComparison.OrdinalIgnoreCase);
            log.Info(nameof(SqlConnectionFactory), $"Using connection string: {safe}");
        }

        public SqlConnection Create() => new SqlConnection(_conn);
    }
}
