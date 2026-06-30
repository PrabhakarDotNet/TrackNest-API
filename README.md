# TrackNest API

A secure and scalable Expense Tracking REST API built using ASP.NET Core 8, Entity Framework Core, SQL Server, and Clean Architecture principles.

TrackNest API enables users to securely manage personal expenses through JWT authentication, refresh token flow, and user-specific expense management.

---

## Features

### Authentication & Security
* User Registration & Login
* JWT Authentication
* HttpOnly Cookie-Based Authentication
* Refresh Token Implementation
* Secure Protected APIs
* User-Specific Data Access
* Automatic Access Token Renewal

### Expense Management
* Create, View, Update, Delete Expenses
* Expense Categorization
* Expense Description & Date Tracking
* Pagination Support
* User-Scoped Data Access (users can only access their own expenses)

### Architecture
* Clean Architecture (API / Application / Domain / Infrastructure)
* Dependency Injection
* Service Layer Pattern
* RESTful API Design
* Interface-based abstractions for testability

### Testing
* xUnit Unit Tests
* EF Core InMemory Database for isolated tests
* Moq for mocking dependencies (IJwtTokenService)
* FluentAssertions for readable assertions
* 15 unit tests covering ExpenseService, AuthService, JwtTokenService and UserProfileService

### Cloud & DevOps
* Deployed on Azure App Service
* Azure SQL Database
* Azure Managed Identity (no credentials in code)
* GitHub Actions CI/CD pipeline (auto-deploy on dev branch)

---

## Technology Stack

* ASP.NET Core 8 Web API
* C#
* Entity Framework Core
* SQL Server / Azure SQL
* JWT Authentication
* Cookie-Based Authentication
* Refresh Tokens
* LINQ
* xUnit / Moq / FluentAssertions
* Azure App Service
* GitHub Actions
* Swagger / OpenAPI
* Visual Studio 2022

---

## Project Structure

```text
TrackNest
│
├── TrackNest.API                  ← Controllers, Middleware, Program.cs
│
├── TrackNest.Application          ← DTOs, Interfaces (Service contracts)
│   ├── DTOs
│   └── Interfaces
│
├── TrackNest.Domain               ← Entities (User, Expense)
│   └── Entities
│
├── TrackNest.Infrastructure       ← EF Core, Services implementation
│   ├── Persistence
│   └── Services
│
└── TrackNest.Tests                ← xUnit Unit Tests
    ├── ExpenseServiceTests.cs
    ├── AuthServiceTests.cs
    ├── JwtTokenServiceTests.cs
    └── UserProfileServiceTests.cs
```

---

## Authentication Flow

1. User registers and creates an account.
2. User logs in with valid credentials.
3. API generates a JWT Access Token + Refresh Token.
4. Tokens stored in secure HttpOnly cookies.
5. Protected endpoints validate the Access Token.
6. When Access Token expires, Refresh Token generates a new one.
7. Users remain authenticated without repeatedly logging in.

---

## Database Entities

### User

| Field                  | Type     |
| ---------------------- | -------- |
| Id                     | int      |
| Username               | string   |
| Email                  | string   |
| Password               | string   |
| RefreshToken           | string   |
| RefreshTokenExpiryTime | datetime |

### Expense

| Field       | Type     |
| ----------- | -------- |
| Id          | int      |
| Amount      | decimal  |
| Category    | string   |
| Description | string   |
| ExpenseDate | datetime |
| UserId      | int      |

---

## API Endpoints

### Authentication

| Method | Endpoint                |
| ------ | ----------------------- |
| POST   | /api/auth/register      |
| POST   | /api/auth/login         |
| POST   | /api/auth/refresh-token |
| POST   | /api/auth/logout        |

### Expenses

| Method | Endpoint           |
| ------ | ------------------ |
| GET    | /api/expenses      |
| GET    | /api/expenses/{id} |
| POST   | /api/expenses      |
| PUT    | /api/expenses/{id} |
| DELETE | /api/expenses/{id} |

---

## Getting Started

### Clone Repository
```bash
git clone https://github.com/your-github-username/TrackNest.git
```

### Restore Packages
```bash
dotnet restore
```

### Apply Database Migrations
```bash
dotnet ef database update --project TrackNest.Infrastructure --startup-project TrackNest.API
```

### Run Application
```bash
dotnet run --project TrackNest.API
```

### Run Unit Tests
```bash
dotnet test
```

### Access Swagger
```text
https://localhost:{port}/swagger
```

---

## Key Concepts Demonstrated

* ASP.NET Core 8 Web API
* Clean Architecture
* Entity Framework Core + Azure SQL
* JWT + Cookie Authentication + Refresh Token Flow
* Dependency Injection + Service Pattern
* RESTful API Design
* User-Based Authorization (users access only their own data)
* xUnit Unit Testing + Moq + FluentAssertions
* Azure App Service + Managed Identity
* GitHub Actions CI/CD

---

## Future Enhancements

* Role-Based Authorization
* Global Exception Handling Middleware
* Serilog Logging
* Docker Support
* Filtering & Search
* Budget Alerts

---

## Author

**Prabhakar Koranga**
Full Stack Developer | .NET | Angular | SQL Server

**GitHub:** [github.com/your-github-username](https://github.com/your-github-username)
