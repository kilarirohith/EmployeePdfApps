# EmployeeCrudPdf – README

A small ASP.NET Core MVC + Web API app that demonstrates:
- **Login & Registration** (cookie for MVC, **JWT in a cookie** for APIs)
- **SQL Server** persistence (Dapper)
- **REST APIs** for Employees, Products, Orders with **Swagger**
- **Validation** (DataAnnotations on the server + unobtrusive client validation)
- **Pagination & Search** (keyword filter + paging)

> Target Framework: **.NET 10 (preview)** or **.NET 8**  
> Packages: Dapper, Microsoft.Data.SqlClient, JwtBearer (8.x), BCrypt.Net-Next, Swashbuckle, Rotativa.AspNetCore (1.4.0)

---

## 1) Login & Registration Module

### What’s implemented
- **Register**: creates a user with `BCrypt` password hashing.
- **Login**: issues a **cookie** (for MVC access) **and** a **JWT** stored in an `access_token` **cookie** (read by the API `JwtBearer` handler).
- **Logout**: clears session, deletes cookies, and signs the user out.

### How to use (step-by-step)
1. **Run the app** (see “Setup & Run” below).
2. In a browser, open `https://localhost:<port>/AccountMvc/Register` and create a user.
3. Then go to `https://localhost:<port>/AccountMvc/Login`, sign in.
   - This sets both:  
     - Cookie for MVC pages  
     - JWT cookie `access_token` for Swagger/API
4. Now open `https://localhost:<port>/swagger`. You can invoke protected API endpoints; the **JwtBearer** handler reads the token from the **cookie** automatically.

> **Note**  
> The “login/logout via Swagger” flow here is implemented by **logging in via the MVC page** which sets the JWT cookie that Swagger uses automatically. If you prefer a pure API login/logout (POST `/api/auth/login|logout`), you can add a minimal Auth API later; the rest of this project is already wired for JWT.

---

## 2) API Creation (+ Swagger)

### Base URL
`https://localhost:<port>/api`

### Employees
- `GET /api/employees?q=&page=1&pageSize=10` – list (paged + search)
- `GET /api/employees/{id}` – get by id
- `POST /api/employees` – create
- `PUT /api/employees/{id}` – update
- `DELETE /api/employees/{id}` – delete

### Products
- `GET /api/products?q=&page=1&pageSize=10`
- `GET /api/products/{id}`
- `POST /api/products`
- `PUT /api/products/{id}`
- `DELETE /api/products/{id}`

### Orders
- `GET /api/orders?q=&page=1&pageSize=10` – list orders (totals computed)
- `GET /api/orders/{id}` – get with items
- `POST /api/orders` – create order with items
- `POST /api/orders/{id}/items` – add item
- `DELETE /api/orders/{id}` – delete order
- `DELETE /api/orders/{orderId}/items/{itemId}` – delete item

### Swagger
- Open `https://localhost:<port>/swagger`
- XML comments are enabled; an `X-Correlation-Id` response header is documented.
- Security: a `cookieAuth` scheme is defined so Swagger calls include your `access_token` cookie (set at Login).

---

## 3) Form Validation

### Server-side
- All DTOs and MVC models use **DataAnnotations** (e.g., `[Required]`, `[EmailAddress]`, `[Range]`, `[StringLength]`).
- Controllers check `ModelState` before saving and return appropriate HTTP codes.

### Client-side
- MVC views reference **jQuery Validate + Unobtrusive** scripts.
- Input fields use `asp-for` and validation helpers (`asp-validation-for`) to surface inline messages.

Result: **same rules** enforced on the server and reflected on the client.

---

## 4) Pagination & Search (with clear explanation)

### What we did
- **Keyword search:** single `q` parameter matched with `LIKE` on relevant columns.  
  - Employees: name/department/email  
  - Products: name/category  
  - Orders: order number
- **Pagination:** `page` (1-based) and `pageSize` with `OFFSET … FETCH` in SQL.

### Why this approach
- **Performance & correctness**: Filtering and paging are pushed down to SQL (Dapper), which is efficient for large datasets.
- **LINQ parity**: The LINQ alternative (fetch all, then filter in memory) is trivial but **not scalable**. For production, **SQL-side filtering** is preferred.  
  If you want a toggle to compare approaches, it’s easy to add a `useLinq=true` query param that:
  1) loads all rows for a user,
  2) applies `Where/Skip/Take` in LINQ,
  3) returns the paged subset.

> Summary: We implemented **server-side** paging/search for scalability and documented how/why a LINQ variant could be wired if needed for demo purposes.

---

## Setup & Run

### Prereqs
- .NET SDK **10 preview** (or **8**)
- **SQL Server** (local dev is fine)
- **SSMS** to run the schema script below

### 1) Configure `appsettings.json`
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=EmployeeCrudDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True;"
  },
  "Jwt": {
    "Key": "dev-only-please-change-this-very-long-secret-key",
    "Issuer": "EmployeeCrudPdf",
    "Audience": "EmployeeCrudPdf",
    "ExpiresHours": "8"
  }
}
