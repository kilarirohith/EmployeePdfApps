using Dapper;
using EmployeeCrudPdf.Exceptions;
using EmployeeCrudPdf.Models;
using EmployeeCrudPdf.Services.Logging;
using Microsoft.Data.SqlClient;

namespace EmployeeCrudPdf.Data
{
    public class ProductRepository : IProductRepository
    {
        private readonly ISqlConnectionFactory _factory;
        private readonly IAppLogger _log;

        public ProductRepository(ISqlConnectionFactory factory, IAppLogger log)
        {
            _factory = factory;
            _log = log;
        }

        public async Task<IEnumerable<Product>> GetAllAsync(int userId, string? keyword = null, int? page = null, int? pageSize = null)
        {
            try
            {
                await using var conn = _factory.Create();
                var where = "WHERE user_id = @userId";
                if (!string.IsNullOrWhiteSpace(keyword))
                    where += " AND (name LIKE @kw OR category LIKE @kw)";

                var order = "ORDER BY id DESC";
                var paging = (page.HasValue && pageSize.HasValue)
                    ? " OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY"
                    : string.Empty;

                var sql = $@"
SELECT id, name, price, category, user_id AS UserId
FROM dbo.products
{where}
{order}{paging};";

                var rows = await conn.QueryAsync<Product>(sql, new
                {
                    userId,
                    kw = $"%{keyword}%",
                    skip = (page.GetValueOrDefault(1) - 1) * pageSize.GetValueOrDefault(10),
                    take = pageSize.GetValueOrDefault(10)
                });

                _log.Info(nameof(GetAllAsync), $"Fetched {rows.Count()} products for user {userId}");
                return rows;
            }
            catch (Exception ex)
            {
                _log.Error(nameof(GetAllAsync), $"Failed to fetch products (user {userId})", ex);
                throw new DatabaseException("Error fetching products", ex);
            }
        }

        public async Task<int> CountAsync(int userId, string? keyword = null)
        {
            try
            {
                await using var conn = _factory.Create();
                var where = "WHERE user_id = @userId";
                if (!string.IsNullOrWhiteSpace(keyword))
                    where += " AND (name LIKE @kw OR category LIKE @kw)";

                var sql = $"SELECT COUNT(*) FROM dbo.products {where};";
                return await conn.ExecuteScalarAsync<int>(sql, new { userId, kw = $"%{keyword}%" });
            }
            catch (Exception ex)
            {
                _log.Error(nameof(CountAsync), $"Failed to count products (user {userId})", ex);
                throw new DatabaseException("Error counting products", ex);
            }
        }

        public async Task<Product> GetByIdAsync(int userId, int id)
        {
            try
            {
                await using var conn = _factory.Create();
                const string sql = @"
SELECT id, name, price, category, user_id AS UserId
FROM dbo.products
WHERE id=@id AND user_id=@userId;";

                var p = await conn.QueryFirstOrDefaultAsync<Product>(sql, new { id, userId });
                if (p == null) throw new NotFoundException("Product", id);
                _log.Info(nameof(GetByIdAsync), $"Product {id} fetched for user {userId}");
                return p;
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _log.Error(nameof(GetByIdAsync), $"Failed to fetch product {id} (user {userId})", ex);
                throw new DatabaseException("Error fetching product", ex);
            }
        }

        public async Task<int> CreateAsync(int userId, Product p)
        {
            try
            {
                await using var conn = _factory.Create();
                const string sql = @"
INSERT INTO dbo.products (user_id, name, price, category)
VALUES (@userId, @Name, @Price, @Category);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

                var id = await conn.ExecuteScalarAsync<int>(sql, new { userId, p.Name, p.Price, p.Category });
                _log.Info(nameof(CreateAsync), $"Created product #{id} for user {userId}");
                return id;
            }
            catch (SqlException ex) when (ex.Number is 2627 or 2601)
            {
                _log.Error(nameof(CreateAsync), $"Duplicate product name '{p.Name}' for user {userId}", ex);
                throw new DuplicateEntityException("Product", "Name", p.Name);
            }
            catch (Exception ex)
            {
                _log.Error(nameof(CreateAsync), $"Failed to create product (user {userId})", ex);
                throw new DatabaseException("Error creating product", ex);
            }
        }

        public async Task<bool> UpdateAsync(int userId, Product p)
        {
            try
            {
                await using var conn = _factory.Create();
                const string sql = @"
UPDATE dbo.products
SET name=@Name, price=@Price, category=@Category
WHERE id=@Id AND user_id=@userId;";

                var rows = await conn.ExecuteAsync(sql, new { userId, p.Id, p.Name, p.Price, p.Category });
                if (rows == 0) throw new NotFoundException("Product", p.Id);
                _log.Info(nameof(UpdateAsync), $"Updated product #{p.Id} for user {userId}");
                return true;
            }
            catch (AppException) { throw; }
            catch (SqlException ex) when (ex.Number is 2627 or 2601)
            {
                _log.Error(nameof(UpdateAsync), $"Duplicate product name '{p.Name}' for user {userId}", ex);
                throw new DuplicateEntityException("Product", "Name", p.Name);
            }
            catch (Exception ex)
            {
                _log.Error(nameof(UpdateAsync), $"Failed to update product #{p.Id} (user {userId})", ex);
                throw new DatabaseException("Error updating product", ex);
            }
        }

        public async Task<bool> DeleteAsync(int userId, int id)
        {
            try
            {
                await using var conn = _factory.Create();
                const string sql = "DELETE FROM dbo.products WHERE id=@id AND user_id=@userId;";
                var rows = await conn.ExecuteAsync(sql, new { id, userId });
                if (rows == 0) throw new NotFoundException("Product", id);
                _log.Info(nameof(DeleteAsync), $"Deleted product #{id} for user {userId}");
                return true;
            }
            catch (AppException) { throw; }
            catch (Exception ex)
            {
                _log.Error(nameof(DeleteAsync), $"Failed to delete product #{id} (user {userId})", ex);
                throw new DatabaseException("Error deleting product", ex);
            }
        }
    }
}
