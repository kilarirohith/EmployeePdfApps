using Dapper;
using EmployeeCrudPdf.Models;

namespace EmployeeCrudPdf.Data
{
    public class UserRepository : IUserRepository
    {
        private readonly ISqlConnectionFactory _factory;
        public UserRepository(ISqlConnectionFactory factory) => _factory = factory;

        public async Task<User?> GetByUsernameAsync(string username)
        {
            await using var conn = _factory.Create();
            return await conn.QueryFirstOrDefaultAsync<User>(
                "SELECT TOP 1 * FROM dbo.Users WHERE Username = @u", new { u = username });
        }

        public async Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail)
        {
            await using var conn = _factory.Create();
            return await conn.QueryFirstOrDefaultAsync<User>(
                "SELECT TOP 1 * FROM dbo.Users WHERE Username = @v OR Email = @v", new { v = usernameOrEmail });
        }

        public async Task<int> CreateAsync(User user)
        {
            await using var conn = _factory.Create();
            var sql = @"INSERT INTO dbo.Users (Username, Email, PasswordHash)
                        VALUES (@Username, @Email, @PasswordHash);
                        SELECT CAST(SCOPE_IDENTITY() as int);";
            return await conn.ExecuteScalarAsync<int>(sql, user);
        }
    }
}