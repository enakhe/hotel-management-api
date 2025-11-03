# Quick Start Guide

## Prerequisites Checklist

- [ ] .NET 9.0 SDK installed
- [ ] Docker Desktop running (for SQL Server & Redis)
- [ ] Visual Studio 2022 or VS Code (optional)

## 🚀 5-Minute Setup

### Step 1: Start Infrastructure Services

```powershell
# From repository root
docker-compose up -d
```

**What this does**:

- Starts SQL Server 2022 on port 1433
- Starts Redis 7 on port 6379

### Step 2: Configure Secrets

**Option A - Automated** (Recommended):

```powershell
cd scripts
.\setup-user-secrets.ps1
```

**Option B - Manual**:

```powershell
cd src/Web

# Generate and set a secure JWT key
dotnet user-secrets set "Jwt:Key" "YOUR-RANDOM-32-CHAR-KEY-HERE"

# Set database connection
dotnet user-secrets set "ConnectionStrings:sql" "Server=localhost;Database=HotelManagementDb;Trusted_Connection=True;MultipleActiveResultSets=true"

# Set Redis connection
dotnet user-secrets set "ConnectionStrings:cache" "localhost:6379"

# Set email password
dotnet user-secrets set "Email:Password" "YOUR-EMAIL-PASSWORD"
```

### Step 3: Run the Application

```powershell
cd src/Web
dotnet run
```

**What happens automatically**:

- ✅ Database migrations applied
- ✅ Default roles & permissions seeded
- ✅ Modules seeded
- ✅ Application starts

### Step 4: Access the Application

**Open your browser**:

- 🌐 API Documentation (Swagger): https://localhost:5001/api
- 🏥 Health Check: https://localhost:5001/health
- 📊 Hangfire Dashboard: https://localhost:5001/hangfire
- 🔔 SignalR Hub: wss://localhost:5001/hubs/notifications

---

## 🎯 First API Call

### 1. Create a Tenant (SuperAdmin)

```bash
POST https://localhost:5001/cp/tenant
Content-Type: application/json
X-Admin-Portal: true

{
  "name": "Test Hotel",
  "identifier": "testhotel",
  "email": "admin@testhotel.com",
  "planId": "PLAN-GUID-HERE"
}
```

### 2. Login

```bash
POST https://localhost:5001/api/v1/auth/login
Content-Type: application/json
X-Tenant-Identifier: testhotel

{
  "email": "admin@testhotel.com",
  "password": "Password123!"
}
```

### 3. Get Users

```bash
GET https://localhost:5001/api/v1/users
Authorization: Bearer YOUR-JWT-TOKEN
X-Tenant-Identifier: testhotel
```

---

## 🧪 Verify Installation

### Check Health

```bash
curl https://localhost:5001/health
```

**Expected Response**:

```json
{
  "status": "Healthy",
  "checks": [
    { "name": "database", "status": "Healthy" },
    { "name": "redis", "status": "Healthy" },
    { "name": "hangfire", "status": "Healthy" },
    { "name": "system", "status": "Healthy" }
  ]
}
```

### Check Database

```bash
# Verify database was created
sqlcmd -S localhost -d HotelManagementDb -E -Q "SELECT COUNT(*) FROM AspNetRoles"
```

**Expected**: 7 roles

### Check Redis

```bash
redis-cli ping
```

**Expected**: PONG

---

## 📚 Next Steps

1. **Create Your First Tenant**

   - See: `docs/MULTI_TENANT_ARCHITECTURE.md`

2. **Configure Application Insights** (Optional)

   - Get connection string from Azure Portal
   - Add to User Secrets: `dotnet user-secrets set "ApplicationInsights:ConnectionString" "YOUR-CONN-STRING"`

3. **Customize Configuration**

   - Edit `appsettings.Development.json` for local overrides
   - See: `docs/SECRETS_MANAGEMENT.md`

4. **Explore the API**
   - Navigate to https://localhost:5001/api
   - Try the interactive API documentation

---

## 🐛 Troubleshooting

### "Cannot connect to database"

**Solution**:

```powershell
# Check if Docker is running
docker ps

# Restart SQL Server container
docker-compose restart sqlserver

# Check connection string
cd src/Web
dotnet user-secrets list
```

### "Cannot connect to Redis"

**Solution**:

```powershell
# Check Redis container
docker ps | findstr redis

# Test Redis connection
redis-cli ping

# Restart Redis
docker-compose restart redis
```

### "User Secrets not found"

**Solution**:

```powershell
cd src/Web

# Initialize user secrets
dotnet user-secrets init

# Re-run setup script
cd..
cd scripts
.\setup-user-secrets.ps1
```

### Build Errors

**Solution**:

```powershell
# Clean and rebuild
dotnet clean
dotnet build
```

---

## 📖 Documentation Index

- **[Secrets Management](./SECRETS_MANAGEMENT.md)** - Managing application secrets
- **[Database Migrations](./DATABASE_MIGRATIONS.md)** - Database schema management
- **[Seed Data](./SEED_DATA.md)** - Initial data seeding
- **[Health Checks](./HEALTH_CHECKS.md)** - Monitoring and health endpoints
- **[Security Headers](./SECURITY_HEADERS.md)** - Security configuration
- **[Tenant Query Filters](./TENANT_QUERY_FILTERS.md)** - Multi-tenant data isolation
- **[Caching Strategy](./CACHING_STRATEGY.md)** - Performance optimization
- **[API Versioning](./API_VERSIONING.md)** - API version management

---

## 🎓 Learning Resources

### Architecture

- Clean Architecture pattern
- CQRS with MediatR
- Multi-tenant design
- Domain-Driven Design

### Technologies

- .NET 9.0 & EF Core 9
- SignalR for real-time
- Redis for caching
- Hangfire for background jobs
- Application Insights for monitoring

---

## ⚙️ Development Tips

### Hot Reload

```powershell
cd src/Web
dotnet watch run
```

Changes to C# code will automatically reload!

### Database Reset

```powershell
cd src/Web
dotnet ef database drop -f
dotnet run  # Auto-migrates and seeds
```

### View Logs

Logs are structured JSON in console. Use jq for pretty printing:

```powershell
dotnet run | jq
```

### Debug SignalR

Open browser console and connect:

```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:5001/hubs/notifications")
  .build();

await connection.start();
console.log("Connected to SignalR!");
```

---

## 🆘 Getting Help

1. Check the documentation in `docs/` folder
2. Review logs in console or Application Insights
3. Check health endpoint: `/health`
4. Review Swagger docs: `/api`

---

**Happy Coding! 🎉**
