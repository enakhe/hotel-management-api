# HotelManagement

A multi-tenant hotel management SaaS platform built with Clean Architecture principles.

The project was generated using the [Clean.Architecture.Solution.Template](https://github.com/jasontaylordev/CleanArchitecture) version 9.0.8.

## Getting Started

### Prerequisites

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [SQL Server 2022](https://www.microsoft.com/sql-server/sql-server-downloads) or Docker
- [Redis](https://redis.io/download) or Docker
- (Optional) [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)

### Quick Start with Docker

Start the required infrastructure services (SQL Server and Redis):

```bash
docker-compose up -d
```

### Development Setup

#### 1. Configure Secrets

**Option A: Quick Setup (Recommended)**

Run the automated setup script:

```powershell
.\scripts\setup-user-secrets.ps1
```

**Option B: Manual Setup**

```bash
cd src/Web

# Set JWT signing key (generate a secure key)
dotnet user-secrets set "Jwt:Key" "YOUR-SECURE-KEY-HERE"

# Set database connection
dotnet user-secrets set "ConnectionStrings:sql" "Server=localhost;Database=HotelManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true"

# Set Redis connection
dotnet user-secrets set "ConnectionStrings:cache" "localhost:6379"

# Set email password
dotnet user-secrets set "Email:Password" "YOUR-EMAIL-PASSWORD"
```

See [Secrets Management Guide](docs/SECRETS_MANAGEMENT.md) for detailed information.

#### 2. Build

Run `dotnet build -tl` to build the solution.

#### 3. Run Database Migrations

```bash
cd src/Web
dotnet ef database update
```

#### 4. Run the Application

```bash
cd src/Web
dotnet watch run
```

Navigate to https://localhost:5001. The application will automatically reload if you change any of the source files.

**API Documentation**: https://localhost:5001/api

## Code Styles & Formatting

The template includes [EditorConfig](https://editorconfig.org/) support to help maintain consistent coding styles for multiple developers working on the same project across various editors and IDEs. The **.editorconfig** file defines the coding styles applicable to this solution.

## Code Scaffolding

The template includes support to scaffold new commands and queries.

Start in the `.\src\Application\` folder.

Create a new command:

```
dotnet new ca-usecase --name CreateTodoList --feature-name TodoLists --usecase-type command --return-type int
```

Create a new query:

```
dotnet new ca-usecase -n GetTodos -fn TodoLists -ut query -rt TodosVm
```

If you encounter the error *"No templates or subcommands found matching: 'ca-usecase'."*, install the template and try again:

```bash
dotnet new install Clean.Architecture.Solution.Template::9.0.8
```

## Test

The solution contains unit, integration, and functional tests.

To run the tests:
```bash
dotnet test
```

## Features

### 🔒 Security
- JWT Authentication with refresh tokens
- Role-based authorization (RBAC)
- Permission-based access control
- OWASP-compliant security headers
- Automatic tenant data isolation
- Enhanced rate limiting (per-tenant & per-endpoint)
- Secrets management (User Secrets + Azure Key Vault)

### ⚡ Performance
- Redis distributed caching
- Database query optimization with indexes
- Output caching
- Connection pooling
- Query result caching

### 📊 Monitoring & Observability
- Application Insights telemetry
- 4 comprehensive health checks (Database, Redis, Hangfire, System)
- Structured JSON logging with correlation IDs
- Performance metrics tracking
- Tenant-aware logging

### 💪 Reliability
- Polly resilience policies (retry, circuit breaker, timeout)
- Graceful degradation
- Health-based auto-recovery
- Transient error handling

### 🔔 Real-Time
- SignalR for instant notifications
- Tenant-scoped broadcasting
- User-specific messages
- Redis backplane for multi-instance support

### 🏢 Multi-Tenancy
- Complete tenant isolation at database level
- Multiple tenant resolution strategies (subdomain, header, query param)
- SuperAdmin bypass for cross-tenant operations
- Tenant-aware caching and logging

### 🔄 API Management
- API versioning (URL, header, query string)
- Comprehensive XML documentation
- Swagger/OpenAPI specification
- RESTful design patterns

## Documentation

Comprehensive guides available in the `docs/` folder:

- 📖 [Quick Start Guide](docs/QUICK_START.md)
- 🔐 [Secrets Management](docs/SECRETS_MANAGEMENT.md)
- 🛡️ [Security Headers](docs/SECURITY_HEADERS.md)
- 🏢 [Tenant Query Filters](docs/TENANT_QUERY_FILTERS.md)
- 🗄️ [Database Migrations](docs/DATABASE_MIGRATIONS.md)
- 🌱 [Seed Data](docs/SEED_DATA.md)
- 🏥 [Health Checks](docs/HEALTH_CHECKS.md)
- ⚡ [Caching Strategy](docs/CACHING_STRATEGY.md)
- 🔄 [API Versioning](docs/API_VERSIONING.md)
- 📋 [Implementation Summary](docs/IMPLEMENTATION_SUMMARY.md)

## Health Endpoints

- `/health` - Comprehensive health status
- `/health/ready` - Readiness probe (Kubernetes)
- `/health/live` - Liveness probe (Kubernetes)

## SignalR Hub

- `/hubs/notifications` - Real-time notification hub

## Architecture

This application implements:
- ✅ Clean Architecture (Onion Architecture)
- ✅ CQRS pattern with MediatR
- ✅ Domain-Driven Design (DDD)
- ✅ Repository pattern
- ✅ Multi-tenant architecture
- ✅ Event-driven architecture

## Technology Stack

- **.NET 9.0** - Latest framework
- **ASP.NET Core 9.0** - Web framework
- **Entity Framework Core 9.0** - ORM
- **SQL Server 2022** - Database
- **Redis 7** - Caching & SignalR backplane
- **MediatR** - CQRS implementation
- **FluentValidation** - Input validation
- **AutoMapper** - Object mapping
- **Hangfire** - Background jobs
- **SignalR** - Real-time communications
- **Polly** - Resilience policies
- **Application Insights** - Monitoring

## Help

To learn more about the template go to the [project website](https://github.com/jasontaylordev/CleanArchitecture). Here you can find additional guidance, request new features, report a bug, and discuss the template with other users.

## Contributing

See [IMPLEMENTATION_COMPLETE.md](docs/IMPLEMENTATION_COMPLETE.md) for details on recent enhancements and implementation status.