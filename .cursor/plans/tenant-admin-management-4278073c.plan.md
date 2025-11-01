<!-- 4278073c-ab52-40c5-825b-946dfe3ebbf9 08aadf60-2818-4b73-a5d1-02fe8b59a82f -->
# Tenant Administrator User Management for SuperAdmin

## Overview

This feature enables SuperAdmins at `/cp/tenant-admins` to manage the primary Administrator account for each tenant. This is distinct from the existing tenant-scoped user management at `/api/v1/users`.

**Key Differentiators:**

- **Existing User Management** (`/api/v1/users`): Tenant admins manage their staff within their tenant (tenant-scoped)
- **New Tenant Admin Management** (`/cp/tenant-admins`): SuperAdmins manage the initial admin account for each tenant (cross-tenant, control plane)

## API Endpoints

All endpoints under `/cp/tenant-admins` with `[Authorize(Roles = "SuperAdmin")]`:

1. **POST /cp/tenant-admins** - Create Tenant Admin

   - Auto-create "Headquarters" branch if tenant has no branches
   - Automatically assign "Administrator" role
   - Validate one admin per tenant
   - Set TenantId on user

2. **GET /cp/tenant-admins** - List all Tenant Admins

   - Pagination support
   - Filter by tenant, status, email
   - Include tenant and branch information

3. **GET /cp/tenant-admins/{id}** - Get Tenant Admin details

   - Include user, tenant, branch, roles information

4. **GET /cp/tenants/{tenantId}/admin** - Get admin for specific tenant

5. **PUT /cp/tenant-admins/{id}** - Update Tenant Admin profile

6. **PATCH /cp/tenant-admins/{id}/reset-password** - Reset password

7. **PATCH /cp/tenant-admins/{id}/activate** - Activate account

8. **PATCH /cp/tenant-admins/{id}/deactivate** - Deactivate account

9. **DELETE /cp/tenant-admins/{id}** - Delete Tenant Admin (with validation)

## Architecture Components

### 1. DTOs (`src/Application/Common/DTOs/`)

Create new DTOs:

- **CreateTenantAdminDto**: FirstName, MiddleName, LastName, Email, PhoneNumber, Password, TenantId
- **UpdateTenantAdminDto**: Id, FirstName, MiddleName, LastName, PhoneNumber
- **TenantAdminDto**: Full details including TenantName, BranchName, Roles, IsActive
- **TenantAdminListDto**: Summary for list views
- **ResetTenantAdminPasswordDto**: UserId, NewPassword

### 2. Commands/Queries (`src/Application/Core/TenantAdminManagement/`)

**Commands:**

- `CreateTenantAdminCommand` + Handler + Validator
- `UpdateTenantAdminCommand` + Handler + Validator
- `ResetTenantAdminPasswordCommand` + Handler + Validator
- `ActivateTenantAdminCommand` + Handler
- `DeactivateTenantAdminCommand` + Handler
- `DeleteTenantAdminCommand` + Handler + Validator

**Queries:**

- `GetTenantAdminByIdQuery` + Handler
- `GetTenantAdminsQuery` + Handler (with pagination/filtering)
- `GetTenantAdminByTenantIdQuery` + Handler

### 3. Service Layer

**Interface** (`src/Application/Common/Interfaces/ITenantAdminService.cs`):

- CreateTenantAdminAsync
- UpdateTenantAdminAsync
- GetTenantAdminByIdAsync
- GetTenantAdminsAsync
- GetTenantAdminByTenantIdAsync
- ResetPasswordAsync
- ActivateAsync
- DeactivateAsync
- DeleteAsync
- ValidateOneTenantAdminAsync

**Implementation** (`src/Infrastructure/Services/TenantAdminService.cs`):

- Use UserManager<ApplicationUser> for user operations
- Use RoleManager<ApplicationRole> for role assignment
- Use ApplicationDbContext for tenant/branch queries
- Use ISuperAdminAuditService for audit logging
- Implement branch auto-creation logic

### 4. Controller (`src/Web/Controllers/SuperAdmin/TenantAdminController.cs`)

- All endpoints with `[Authorize(Roles = "SuperAdmin")]`
- Follows RESTful conventions
- Returns appropriate status codes
- Comprehensive XML documentation

### 5. AutoMapper Profiles

Update `src/Application/Common/Mappings/` to include:

- CreateTenantAdminDto → ApplicationUser
- ApplicationUser → TenantAdminDto
- UpdateTenantAdminDto → ApplicationUser

### 6. Validators

FluentValidation for all commands:

- Email format and uniqueness
- Password complexity (min 8 chars)
- Required fields validation
- Tenant existence validation
- One admin per tenant validation

## Key Business Logic

### Creating Tenant Admin:

1. Validate tenant exists and is active
2. Check tenant doesn't already have an admin
3. Check email doesn't already exist
4. Check if tenant has any branches:

   - If NO branches → Auto-create "Headquarters" branch
   - If branches exist → Use first active branch or create HQ

5. Create ApplicationUser with TenantId and BranchId
6. Assign "Administrator" role automatically
7. Log audit entry via ISuperAdminAuditService

### Auto-Branch Creation:

```csharp
// Create "Headquarters" branch if needed
var branch = new Branch {
    TenantId = tenantId,
    Name = "Headquarters",
    IsActive = true,
    TimeZone = tenant.TimeZone,
    CurrencyCode = tenant.CurrencyCode,
    Address = tenant.Address,
    Email = tenant.Email,
    ContactNumber = tenant.ContactNumber
};
```

### Validation Rules:

- Tenant must exist and be active
- Email must be unique across ALL tenants (global uniqueness)
- Only ONE admin per tenant (check before create)
- Cannot delete admin if they're the only admin for active tenant

## Security & Audit

- All operations logged via `ISuperAdminAuditService`
- Include SuperAdmin ID, Tenant ID, and operation details
- Track IP address and user agent
- Password reset creates audit log

## Files to Create/Modify

**New Files:**

- `src/Application/Common/DTOs/CreateTenantAdminDto.cs`
- `src/Application/Common/DTOs/UpdateTenantAdminDto.cs`
- `src/Application/Common/DTOs/TenantAdminDto.cs`
- `src/Application/Common/DTOs/ResetTenantAdminPasswordDto.cs`
- `src/Application/Common/Interfaces/ITenantAdminService.cs`
- `src/Infrastructure/Services/TenantAdminService.cs`
- `src/Application/Core/TenantAdminManagement/Commands/CreateTenantAdmin.cs`
- `src/Application/Core/TenantAdminManagement/Commands/UpdateTenantAdmin.cs`
- `src/Application/Core/TenantAdminManagement/Commands/ResetTenantAdminPassword.cs`
- `src/Application/Core/TenantAdminManagement/Commands/ActivateTenantAdmin.cs`
- `src/Application/Core/TenantAdminManagement/Commands/DeactivateTenantAdmin.cs`
- `src/Application/Core/TenantAdminManagement/Commands/DeleteTenantAdmin.cs`
- `src/Application/Core/TenantAdminManagement/Queries/GetTenantAdminById.cs`
- `src/Application/Core/TenantAdminManagement/Queries/GetTenantAdmins.cs`
- `src/Application/Core/TenantAdminManagement/Queries/GetTenantAdminByTenantId.cs`
- `src/Web/Controllers/SuperAdmin/TenantAdminController.cs`

**Modify Files:**

- `src/Application/Common/Mappings/MappingProfile.cs` - Add TenantAdmin mappings
- `src/Infrastructure/DependencyInjection.cs` - Register ITenantAdminService
- `src/Application/Common/Interfaces/IUserRepository.cs` - Add GetByTenantIdAsync if needed

## Dependencies

Leverage existing infrastructure:

- `UserManager<ApplicationUser>` (ASP.NET Identity)
- `RoleManager<ApplicationRole>` (ASP.NET Identity)
- `ApplicationDbContext` (EF Core)
- `ISuperAdminAuditService` (Audit logging)
- `AutoMapper` (DTO mapping)
- `FluentValidation` (Validation)
- `MediatR` (CQRS)

### To-dos

- [ ] Create DTOs: CreateTenantAdminDto, UpdateTenantAdminDto, TenantAdminDto, ResetTenantAdminPasswordDto
- [ ] Create ITenantAdminService interface with all required methods
- [ ] Implement TenantAdminService with branch auto-creation logic and validation
- [ ] Create all Commands with Handlers and Validators (Create, Update, Reset, Activate, Deactivate, Delete)
- [ ] Create all Queries with Handlers (GetById, GetAll, GetByTenantId)
- [ ] Create TenantAdminController with all 9 endpoints and proper documentation
- [ ] Add AutoMapper profiles for TenantAdmin DTOs
- [ ] Register ITenantAdminService in DependencyInjection.cs
- [ ] Test all endpoints using Swagger/Postman to ensure proper functionality