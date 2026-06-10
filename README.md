# TrackNest

A secure and scalable Expense Tracking REST API built using ASP.NET Core 8, Entity Framework Core, SQL Server, and Clean Architecture principles.

TrackNest enables users to securely manage personal expenses through authentication, authorization, and user-specific expense management.

---

## Features

### Authentication & Security

* User Registration
* User Login
* JWT Authentication
* HttpOnly Cookie-Based Authentication
* Refresh Token Implementation
* Secure Protected APIs
* User-Specific Data Access
* Automatic Access Token Renewal

### Expense Management

* Create Expense
* View Expenses
* Update Expenses
* Delete Expenses
* Expense Categorization
* Expense Description Tracking
* Expense Date Management

### Architecture

* Clean Architecture
* Dependency Injection
* Repository Pattern
* Service Layer
* RESTful API Design
* Entity Framework Core

---

## Technology Stack

### Backend

* ASP.NET Core 8 Web API
* C#
* Entity Framework Core
* SQL Server
* JWT Authentication
* Cookie-Based Authentication
* Refresh Tokens
* LINQ

### Tools & Technologies

* Visual Studio 2022
* Swagger / OpenAPI
* Git
* GitHub

---

## Project Structure

```text
TrackNest
│
├── TrackNest.API
│
├── TrackNest.Application
│   ├── DTOs
│   ├── Interfaces
│   └── Services
│
├── TrackNest.Domain
│   └── Entities
│
└── TrackNest.Infrastructure
    ├── Persistence
    ├── Services
    └── Repositories
```

---

## Authentication Flow

1. User registers and creates an account.
2. User logs in with valid credentials.
3. API generates a JWT Access Token.
4. API generates a Refresh Token.
5. Tokens are stored in secure HttpOnly cookies.
6. Protected endpoints validate the Access Token.
7. When the Access Token expires, the Refresh Token is used to generate a new Access Token.
8. Users remain authenticated without repeatedly logging in.

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

## API Documentation & Testing

The API is documented and tested using Swagger/OpenAPI.

Swagger provides:

* Interactive API documentation
* Endpoint testing
* Request and Response visualization
* Authentication testing
* API contract verification

After running the application, Swagger can be accessed using:

```text
https://localhost:{port}/swagger
```

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

---

## Key Concepts Demonstrated

* ASP.NET Core Web API
* Clean Architecture
* Entity Framework Core
* SQL Server
* JWT Authentication
* Cookie-Based Authentication
* Refresh Token Flow
* Dependency Injection
* Repository Pattern
* RESTful API Design
* User-Based Authorization
* Secure Authentication Practices

---

## Future Enhancements

* Role-Based Authorization
* Global Exception Handling Middleware
* Serilog Logging
* Pagination
* Filtering & Search
* Docker Support
* RabbitMQ Integration
* Azure Deployment
* CI/CD Pipeline
* Unit Testing

---

## Author

**Prabhakar Koranga**

Full Stack Developer

**Tech Stack:** .NET | Angular | SQL Server
