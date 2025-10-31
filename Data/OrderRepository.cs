using Dapper;
using EmployeeCrudPdf.Exceptions;
using EmployeeCrudPdf.Models;
using EmployeeCrudPdf.Services.Logging;

namespace EmployeeCrudPdf.Data
{
    public class OrderRepository : IOrderRepository
    {
        private readonly ISqlConnectionFactory _factory;
        private readonly IAppLogger _log;

        public OrderRepository(ISqlConnectionFactory factory, IAppLogger log)
        {
            _factory = factory;
            _log = log;
        }

        public async Task<(IEnumerable<Order> Items, int Total)> ListAsync(int userId, string? q, int page, int pageSize)
        {
            await using var conn = _factory.Create();

            var where = "WHERE o.user_id = @userId";
            if (!string.IsNullOrWhiteSpace(q))
                where += " AND o.order_no LIKE @kw";

            // Paged order headers
            var sql = $@"
WITH paged AS (
  SELECT o.id, o.order_no AS OrderNo, o.created_at AS CreatedAt, o.user_id AS UserId
  FROM dbo.orders o
  {where}
  ORDER BY o.id DESC
  OFFSET @skip ROWS FETCH NEXT @take ROWS ONLY
)
SELECT p.id, p.OrderNo, p.CreatedAt, p.UserId,
       ISNULL(SUM(oi.qty * oi.price),0) AS Total,
       ISNULL(SUM(oi.qty),0) AS ItemsCount
FROM paged p
LEFT JOIN dbo.order_items oi ON oi.order_id = p.id AND oi.user_id = @userId
GROUP BY p.id, p.OrderNo, p.CreatedAt, p.UserId
ORDER BY p.id DESC;

SELECT COUNT(*) FROM dbo.orders o {where};";

            using var multi = await conn.QueryMultipleAsync(sql, new
            {
                userId,
                kw = $"%{q}%",
                skip = (page - 1) * pageSize,
                take = pageSize
            });

            var list = (await multi.ReadAsync<Order>()).ToList();
            // The first read projected as Order with Total/ItemsCount columns; add them:
            // Because Dapper maps by names, we can define a tiny local mapping
            // but for brevity weâ€™ll keep Order and compute totals in SELECT, then copy

            // Better: read into an anonymous and convert:
            // To keep concise, assume Order has Total from SELECT (we defined property)
            var total = await multi.ReadFirstAsync<int>();

            _log.Info(nameof(ListAsync), $"Orders fetched: {list.Count} for user {userId} (q='{q}', page={page})");
            return (list, total);
        }

        public async Task<Order> GetAsync(int userId, int orderId)
        {
            await using var conn = _factory.Create();

            const string headSql = @"
SELECT id, order_no AS OrderNo, created_at AS CreatedAt, user_id AS UserId
FROM dbo.orders WHERE id=@orderId AND user_id=@userId;";

            var order = await conn.QueryFirstOrDefaultAsync<Order>(headSql, new { orderId, userId });
            if (order == null)
            {
                _log.Warn(nameof(GetAsync), $"Order {orderId} not found for user {userId}");
                throw new NotFoundException("Order", orderId);
            }

            const string itemsSql = @"
SELECT oi.id, oi.order_id AS OrderId, oi.qty, oi.price, oi.product_id AS ProductId, oi.user_id AS UserId,
       p.name AS ProductName, p.category
FROM dbo.order_items oi
JOIN dbo.products p ON p.id = oi.product_id AND p.user_id = oi.user_id
WHERE oi.order_id = @orderId AND oi.user_id = @userId
ORDER BY oi.id;";

            var items = await conn.QueryAsync<OrderItem>(itemsSql, new { orderId, userId });
            order.Items = items.ToList();

            _log.Info(nameof(GetAsync), $"Order {orderId} fetched for user {userId} with {order.Items.Count} items");
            return order;
        }

        public async Task<int> CreateOrderAsync(int userId, string orderNo)
        {
            await using var conn = _factory.Create();
            const string sql = @"
INSERT INTO dbo.orders (user_id, order_no)
VALUES (@userId, @orderNo);
SELECT CAST(SCOPE_IDENTITY() AS INT);";
            var id = await conn.ExecuteScalarAsync<int>(sql, new { userId, orderNo });
            _log.Info(nameof(CreateOrderAsync), $"Created order #{id} ({orderNo}) for user {userId}");
            return id;
        }

        public async Task AddItemAsync(int userId, int orderId, int productId, int qty, decimal price)
        {
            await using var conn = _factory.Create();

            // Validate order ownership
            var oid = await conn.ExecuteScalarAsync<int?>(
                "SELECT id FROM dbo.orders WHERE id=@orderId AND user_id=@userId;",
                new { orderId, userId });
            if (oid is null) throw new NotFoundException("Order", orderId);

            // Validate product ownership & (optionally) pull current price
            var prod = await conn.QueryFirstOrDefaultAsync<(int Id, decimal Price)>(
                "SELECT id, price FROM dbo.products WHERE id=@productId AND user_id=@userId;",
                new { productId, userId });
            if (prod.Id == 0) throw new NotFoundException("Product", productId);
            var usePrice = price > 0 ? price : prod.Price;

            const string ins = @"
INSERT INTO dbo.order_items (order_id, product_id, qty, price, user_id)
VALUES (@orderId, @productId, @qty, @price, @userId);";
            await conn.ExecuteAsync(ins, new { orderId, productId, qty, price = usePrice, userId });

            _log.Info(nameof(AddItemAsync), $"Added item product={productId} x{qty} to order {orderId} for user {userId}");
        }

        public async Task<bool> DeleteAsync(int userId, int orderId)
        {
            await using var conn = _factory.Create();
            // delete items then header
            var tx = await conn.BeginTransactionAsync();
            try
            {
                var delItems = await conn.ExecuteAsync(
                    "DELETE FROM dbo.order_items WHERE order_id=@orderId AND user_id=@userId;",
                    new { orderId, userId }, tx);

                var rows = await conn.ExecuteAsync(
                    "DELETE FROM dbo.orders WHERE id=@orderId AND user_id=@userId;",
                    new { orderId, userId }, tx);

                await tx.CommitAsync();
                if (rows == 0)
                {
                    _log.Warn(nameof(DeleteAsync), $"No order deleted #{orderId} for user {userId}");
                    throw new NotFoundException("Order", orderId);
                }
                _log.Info(nameof(DeleteAsync), $"Deleted order #{orderId} (and {delItems} items) for user {userId}");
                return true;
            }
            catch (AppException) { await tx.RollbackAsync(); throw; }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                _log.Error(nameof(DeleteAsync), $"Failed deleting order #{orderId} for user {userId}", ex);
                throw new DatabaseException("Error deleting order", ex);
            }
        }

        public async Task<bool> DeleteItemAsync(int userId, int orderId, int itemId)
        {
            await using var conn = _factory.Create();
            var rows = await conn.ExecuteAsync(
                "DELETE FROM dbo.order_items WHERE id=@itemId AND order_id=@orderId AND user_id=@userId;",
                new { itemId, orderId, userId });
            if (rows == 0)
            {
                _log.Warn(nameof(DeleteItemAsync), $"No item deleted #{itemId} from order #{orderId} for user {userId}");
                throw new NotFoundException("OrderItem", itemId);
            }
            _log.Info(nameof(DeleteItemAsync), $"Deleted item #{itemId} from order #{orderId} for user {userId}");
            return true;
        }
    }
}
