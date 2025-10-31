# 🧩 Employee Management CRUD System

A full-stack web application that allows users to **register, log in, and perform CRUD operations** (Create, Read, Update, Delete) on employee and product data.  
It is built with **ASP.NET Core MVC**, **Entity Framework / Dapper**, and **SQL Server**, following a **layered (clean) architecture**.

---

## 🚀 Features

- 🔐 User authentication (Login / Register)
- 👨‍💼 Employee management (Add, View, Edit, Delete)
- 📦 Product management with search & pagination
- 📄 PDF report generation
- 🧱 Clean layered architecture (Controller → Repository → Database)
- 🗃 SQL Server integration using Dapper / EF
- 🎨 Simple and responsive Razor-based UI

---

## 🗂 Project Structure
EmployeeCrudPdf/
│
├── Controllers/
│ ├── AccountController.cs # Handles Login, Register, Logout
│ ├── EmployeesController.cs # Handles Employee CRUD operations
│ └── ProductsController.cs # Handles Product CRUD, search, pagination
│
├── Models/
│ ├── Employee.cs # Employee entity
│ ├── Product.cs # Product entity
│ ├── User.cs # User entity for authentication
│
├── Data/
│ ├── IEmployeeRepository.cs # Interface for Employee data access
│ ├── IProductRepository.cs # Interface for Product data access
│ ├── EmployeeRepository.cs # Dapper implementation for Employee
│ ├── ProductRepository.cs # Dapper implementation for Product
│ ├── ApplicationDbContext.cs # EF context (optional)
│
├── Views/
│ ├── Account/ # Login, Register pages
│ ├── Employees/ # Index, Create, Edit, Delete
│ ├── Products/ # Index, Create, Edit, Delete
│ └── Shared/ # Layout and partial views
│
├── wwwroot/ # Static assets (CSS, JS, images)
│
├── appsettings.json # DB connection string
├── Program.cs # Application entry point
├── Startup.cs # Middleware, routing, dependency injection
└── README.md
- **Controllers** handle requests and responses.
- **Repositories** abstract database logic (using Dapper / EF).
- **Models** represent data entities.
- **Views** render UI.
- **Dependency Injection** is used to inject repository interfaces into controllers.

---

## 🧱 Class-to-Class Mapping (Implementation Overview)

| Layer         | Class / File                      | Responsibility                                                            | Depends On                     |
|----------------|----------------------------------|---------------------------------------------------------------------------|--------------------------------|
| Controller     | `AccountController.cs`           | Handles Login, Register, and Logout.                                      | `UserRepository`, `Session`    |
| Controller     | `EmployeesController.cs`         | CRUD for Employee; uses repository methods.                               | `IEmployeeRepository`          |
| Controller     | `ProductsController.cs`          | CRUD + Search + Pagination for Products.                                  | `IProductRepository`           |
| Repository     | `EmployeeRepository.cs`          | Database CRUD logic for Employee table.                                   | Dapper / SQL Connection        |
| Repository     | `ProductRepository.cs`           | Database CRUD logic for Product table.                                    | Dapper / SQL Connection        |
| Model          | `Employee.cs`                    | Defines Employee properties (Id, Name, Email, Salary, etc.)               | —                              |
| Model          | `Product.cs`                     | Defines Product properties (Id, Name, Category, Price, etc.)              | —                              |
| Model          | `User.cs`                        | Defines user for authentication (Id, Username, Password).                 | —                              |
| View           | `Views/Employees/Index.cshtml`   | Displays list of employees.                                               | `EmployeesController`          |
| View           | `Views/Products/Index.cshtml`    | Displays list of products with pagination and search bar.                 | `ProductsController`           |

---

## 🧰 Technologies Used

| Layer | Technology |
|-------|-------------|
| Backend | ASP.NET Core MVC, C# |
| Database | SQL Server (LocalDB or SSMS connection) |
| ORM / Data Access | Dapper or Entity Framework Core |
| Frontend | Razor Views, Bootstrap |
| Authentication | ASP.NET Core Session & Cookie Auth |
| PDF Generation | Rotativa / iTextSharp (optional) |
| Tools | Visual Studio 2022, SSMS |

---

## ⚙️ Setup & Configuration

### Prerequisites
- **Visual Studio 2022 or later**
- **SQL Server / SSMS**
- **.NET 6 or later**

### Steps
1. Clone the repository:
   ```bash
   git clone https://github.com/<your-username>/EmployeeCrudPdf.git
   cd EmployeeCrudPdf

