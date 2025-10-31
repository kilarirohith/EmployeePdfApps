using System.Diagnostics;
using Dapper;
using EmployeeCrudPdf.Models;                 // << correct
using EmployeeCrudPdf.Services.Logging;       // << correct
using EmployeeCrudPdf.Exceptions;             // << correct
using Microsoft.Data.SqlClient;               // << required
using Microsoft.Extensions.Configuration;

namespace EmployeeCrudPdf.Data
{
    public class EmployeeRepository : IEmployeeRepository
    {
        private readonly ISqlConnectionFactory _factory;
        private readonly IAppLogger _log;
        private readonly double _slowSeconds;

        public EmployeeRepository(ISqlConnectionFactory factory, IAppLogger log, IConfiguration cfg)
        {
            _factory = factory;
            _log = log;
            _slowSeconds = cfg.GetValue<double?>("Performance:SlowQuerySeconds") ?? 2.0;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync(int userId, string? keyword = null, int? page = null, int? pageSize = null)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await using var conn = _factory.Create();

                var where = "WHERE user_id = @userId";
                if (!string.IsNullOrWhiteSpace(keyword))
                    where += " AND (name LIKE @kw OR department LIKE @kw OR email LIKE @kw)";

                var order = "ORDER BY id DESC";
                var paging = (page.HasValue && pageSize.HasValue)
                    ? " OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY"
                    : string.Empty;

                var sql = $@"
SELECT id, name, department, email, salary, user_id AS UserId
FROM dbo.employees
{where}
{order}{paging};";

                var rows = await conn.QueryAsync<Employee>(sql, new
                {
                    userId,
                    kw = $"%{keyword}%",
                    skip = (page.GetValueOrDefault(1) - 1) * pageSize.GetValueOrDefault(10),
                    take = pageSize.GetValueOrDefault(10)
                });

                sw.Stop();
                if (sw.Elapsed.TotalSeconds > _slowSeconds)
                    _log.Warn(nameof(GetAllAsync), $"Slow query ({sw.Elapsed.TotalSeconds:F2}s) for GetAllAsync");

                _log.Info(nameof(GetAllAsync), $"Fetched {rows.Count()} employees for user {userId}");
                return rows;
            }
            catch (Exception ex)
            {
                sw.Stop();
                _log.Error(nameof(GetAllAsync), $"Failed to fetch employees (user {userId})", ex);
                throw new DatabaseException("Error fetching employees", ex);
            }
        }

        public async Task<int> CountAsync(int userId, string? keyword = null)
        {
            try
            {
                await using var conn = _factory.Create();
                var where = "WHERE user_id = @userId";
                if (!string.IsNullOrWhiteSpace(keyword))
                    where += " AND (name LIKE @kw OR department LIKE @kw OR email LIKE @kw)";
                var sql = $"SELECT COUNT(*) FROM dbo.employees {where};";
                return await conn.ExecuteScalarAsync<int>(sql, new { userId, kw = $"%{keyword}%" });
            }
            catch (Exception ex)
            {
                _log.Error(nameof(CountAsync), $"Failed to count employees (user {userId})", ex);
                throw new DatabaseException("Error counting employees", ex);
            }
        }

        public async Task<Employee> GetByIdAsync(int userId, int id)
        {
            try
            {
                await using var conn = _factory.Create();
                const string sql = @"
SELECT id, name, department, email, salary, user_id AS UserId
FROM dbo.employees
WHERE id=@id AND user_id=@userId;";
                var emp = await conn.QueryFirstOrDefaultAsync<Employee>(sql, new { id, userId });
                if (emp == null)
                {
                    _log.Warn(nameof(GetByIdAsync), $"Employee {id} not found for user {userId}");
                    throw new NotFoundException("Employee", id);
                }
                _log.Info(nameof(GetByIdAsync), $"Employee {id} fetched for user {userId}");
                return emp;
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _log.Error(nameof(GetByIdAsync), $"Failed to fetch employee {id} (user {userId})", ex);
                throw new DatabaseException("Error fetching employee", ex);
            }
        }

        public async Task<int> CreateAsync(int userId, Employee emp)
        {
            const string sql = @"
INSERT INTO dbo.employees (user_id, name, department, email, salary)
VALUES (@userId, @Name, @Department, @Email, @Salary);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            try
            {
                await using var conn = _factory.Create();
                var id = await conn.ExecuteScalarAsync<int>(sql,
                    new { userId, emp.Name, emp.Department, emp.Email, emp.Salary });
                _log.Info(nameof(CreateAsync), $"Created employee #{id} ({emp.Email}) for user {userId}");
                return id;
            }
            catch (SqlException ex) when (ex.Number is 2627 or 2601)
            {
                _log.Error(nameof(CreateAsync), $"Duplicate email {emp.Email} for user {userId}", ex);
                throw new DuplicateEntityException("Employee", "Email", emp.Email);
            }
            catch (Exception ex)
            {
                _log.Error(nameof(CreateAsync), $"Failed to create employee ({emp.Email}) for user {userId}", ex);
                throw new DatabaseException("Error creating employee", ex);
            }
        }

        public async Task<bool> UpdateAsync(int userId, Employee emp)
        {
            const string sql = @"
UPDATE dbo.employees
SET name=@Name, department=@Department, email=@Email, salary=@Salary
WHERE id=@Id AND user_id=@userId;";
            try
            {
                await using var conn = _factory.Create();
                var rows = await conn.ExecuteAsync(sql,
                    new { userId, emp.Id, emp.Name, emp.Department, emp.Email, emp.Salary });
                if (rows == 0)
                {
                    _log.Warn(nameof(UpdateAsync), $"No rows updated for employee #{emp.Id} (user {userId})");
                    throw new NotFoundException("Employee", emp.Id);
                }
                _log.Info(nameof(UpdateAsync), $"Updated employee #{emp.Id} (user {userId})");
                return true;
            }
            catch (AppException) { throw; }
            catch (SqlException ex) when (ex.Number is 2627 or 2601)
            {
                _log.Error(nameof(UpdateAsync), $"Duplicate email {emp.Email} for user {userId}", ex);
                throw new DuplicateEntityException("Employee", "Email", emp.Email);
            }
            catch (Exception ex)
            {
                _log.Error(nameof(UpdateAsync), $"Failed to update employee #{emp.Id} (user {userId})", ex);
                throw new DatabaseException("Error updating employee", ex);
            }
        }

        public async Task<bool> DeleteAsync(int userId, int id)
        {
            try
            {
                await using var conn = _factory.Create();
                const string sql = "DELETE FROM dbo.employees WHERE id=@id AND user_id=@userId;";
                var rows = await conn.ExecuteAsync(sql, new { id, userId });
                if (rows == 0)
                {
                    _log.Warn(nameof(DeleteAsync), $"No rows deleted for employee #{id} (user {userId})");
                    throw new NotFoundException("Employee", id);
                }
                _log.Info(nameof(DeleteAsync), $"Deleted employee #{id} (user {userId})");
                return true;
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _log.Error(nameof(DeleteAsync), $"Failed to delete employee #{id} (user {userId})", ex);
                throw new DatabaseException("Error deleting employee", ex);
            }
        }
    }
}
