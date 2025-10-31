using Dapper;
using Microsoft.Data.SqlClient;
using EmployeeCrudPdf.Models;

namespace EmployeeCrudPdf.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly ISqlConnectionFactory _factory;
        public UserRepository(ISqlConnectionFactory factory) => _factory = factory;

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            await using var conn = _factory.Create();
            const string sql = @"
SELECT id, username, email, password_hash AS PasswordHash, created_at AS CreatedAt
FROM dbo.users
WHERE username=@v OR email=@v;";
            return await conn.QueryFirstOrDefaultAsync<User>(sql, new { v = usernameOrEmail });
        }

        public async Task<int> CreateAsync(User u)
        {
            await using var conn = _factory.Create();
            const string sql = @"
INSERT INTO dbo.users (username, email, password_hash, created_at)
VALUES (@Username, @Email, @PasswordHash, @CreatedAt);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            return await conn.ExecuteScalarAsync<int>(sql, u);
        }
    }
}
