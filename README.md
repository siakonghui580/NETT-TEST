# NETT-TEST (.NET 8 Web API)

This is a sample .NET 8 Web API for managing users, with support for:
- Redis caching
- RabbitMQ integration for messaging
- SQL database using EF Core
- System wide logging (log is in Web.Host > bin > Debug > net8.0 > Logs)

---

## 🚀 How to Run

### Prerequisites
- [.NET 8 SDK](https://dotnet.microsoft.com/download)
- Docker (for Redis)
- RabbitMQ is using cloud RabbitMQ
- SQL Server

### Step-by-step

# Run Redis via Docker
Open PowerShell or Terminal:
docker run -d --name redis-local -p 6379:6379 redis

# Apply migrations
1. Open NETT.sln
2. Change DB connection string in Migrator project's appsettings.json
3. Change startup project to Migrator
4. Run Migrator, input "Y" to console to apply migrations to DB
5. Change DB connection string in Web.Host project's appsettings.json
3. Change startup project to Web.Host and run


🔧 Endpoints
Method	URL	Description
GET	/api/users/{id}	Get user by ID (cached)
POST /api/users	Create user
PUT	/api/users/{id}	Update user (invalidates cache)
DELETE	/api/users/{id}	Delete user (invalidates cache)

📦 Assumptions
Caching is only used for GetUserByIdAsync and optionally for filtered lists.

RabbitMQ is used to publish a message on user creation, updates and deletions (e.g. UserCreated).

Redis uses a key naming convention: user:{id}.

🏗️ Architectural Choices
Dependency Injection (DI) is used for decoupled service layers.

Repository pattern for data access abstraction.

RabbitMQ for eventual consistency and message-based extensibility.

Redis for read performance on hot GET /users/{id} calls.

EF Core is used for database access with migrations.

DTOs separate API contracts from domain entities.

Swagger is added for API exploration.