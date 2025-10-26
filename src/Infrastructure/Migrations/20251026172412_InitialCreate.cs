using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    BillingCycle = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsPopular = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    Modules = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plans", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoomNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RoomName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    RoomType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Floor = table.Column<int>(type: "int", nullable: false),
                    MaxOccupancy = table.Column<int>(type: "int", nullable: false),
                    BaseRate = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    IsAvailable = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Amenities = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValue: "Available"),
                    LastMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    NextMaintenanceDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SuperAdminAuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SuperAdminId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Username = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    TargetType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TargetId = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Details = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Changes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuperAdminAuditLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Tenants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Identifier = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "UTC"),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "USD"),
                    LanguageCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true, defaultValue: "en"),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    SubscriptionStartDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubscriptionEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SubscriptionPlan = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "Basic"),
                    LicenseStatus = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValue: "Trial"),
                    LicenseExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    LastLicenseCheck = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxUsers = table.Column<int>(type: "int", nullable: false, defaultValue: 10),
                    MaxBranches = table.Column<int>(type: "int", nullable: false, defaultValue: 1),
                    MaxRooms = table.Column<int>(type: "int", nullable: false, defaultValue: 100),
                    MaxReservations = table.Column<int>(type: "int", nullable: false, defaultValue: 1000),
                    DatabaseConnectionString = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    DatabaseProvider = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true, defaultValue: "SqlServer"),
                    UseSharedDatabase = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    FeatureFlags = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Industry = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Country = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Region = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    BillingContactName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BillingEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    BillingAddress = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Created = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    LastModified = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    LastModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tenants", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermissions",
                columns: table => new
                {
                    ApplicationRoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermissions", x => new { x.ApplicationRoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermissions_AspNetRoles_ApplicationRoleId",
                        column: x => x.ApplicationRoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermissions_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Included = table.Column<bool>(type: "bit", nullable: false),
                    Limit = table.Column<int>(type: "int", nullable: true),
                    Unit = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanFeatures_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlanLimits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MaxUsers = table.Column<int>(type: "int", nullable: false),
                    MaxBranches = table.Column<int>(type: "int", nullable: false),
                    MaxRooms = table.Column<int>(type: "int", nullable: false),
                    MaxReservations = table.Column<int>(type: "int", nullable: false),
                    MaxStorageGB = table.Column<int>(type: "int", nullable: false),
                    ApiRateLimit = table.Column<int>(type: "int", nullable: false),
                    SupportLevel = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SLA = table.Column<decimal>(type: "decimal(5,2)", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanLimits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanLimits_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Reservations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReservationNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RoomId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    GuestName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GuestEmail = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    GuestPhone = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    CheckInDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckOutDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NumberOfGuests = table.Column<int>(type: "int", nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PaidAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValue: "Confirmed"),
                    SpecialRequests = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PaymentStatus = table.Column<string>(type: "nvarchar(450)", nullable: false, defaultValue: "Pending"),
                    PaymentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PaymentMethod = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    CancelledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CancellationReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CancellationFee = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedBy = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedBy = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reservations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Reservations_Rooms_RoomId",
                        column: x => x.RoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Branches",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    TimeZone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CurrencyCode = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Branches", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Branches_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TenantFeatures",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FeatureName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsEnabled = table.Column<bool>(type: "bit", nullable: false, defaultValue: false),
                    EnabledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DisabledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantFeatures", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantFeatures_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    MiddleName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ProfilePicture = table.Column<byte[]>(type: "varbinary(max)", nullable: true),
                    Gender = table.Column<int>(type: "int", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BranchId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SecurityStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "bit", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "bit", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "bit", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Branches_BranchId",
                        column: x => x.BranchId,
                        principalTable: "Branches",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AspNetUsers_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ClaimType = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ClaimValue = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderKey = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RoleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LoginProvider = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    Value = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogs",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Action = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    EntityId = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    IpAddress = table.Column<string>(type: "nvarchar(45)", maxLength: 45, nullable: true),
                    UserAgent = table.Column<string>(type: "nvarchar(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogs_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AuditLogDetails",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AuditLogId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PropertyName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    OriginalValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    NewValue = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AuditLogDetails", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AuditLogDetails_AuditLogs_AuditLogId",
                        column: x => x.AuditLogId,
                        principalTable: "AuditLogs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true,
                filter: "[NormalizedName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_BranchId",
                table: "AspNetUsers",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_TenantId",
                table: "AspNetUsers",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true,
                filter: "[NormalizedUserName] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogDetails_AuditLogId",
                table: "AuditLogDetails",
                column: "AuditLogId");

            migrationBuilder.CreateIndex(
                name: "IX_AuditLogs_UserId",
                table: "AuditLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Branches_TenantId",
                table: "Branches",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanFeatures_PlanId",
                table: "PlanFeatures",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PlanLimits_PlanId",
                table: "PlanLimits",
                column: "PlanId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Plans_IsActive",
                table: "Plans",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_IsPopular",
                table: "Plans",
                column: "IsPopular");

            migrationBuilder.CreateIndex(
                name: "IX_Plans_Name",
                table: "Plans",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_BranchId",
                table: "Reservations",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CheckInDate",
                table: "Reservations",
                column: "CheckInDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_CheckOutDate",
                table: "Reservations",
                column: "CheckOutDate");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_GuestEmail",
                table: "Reservations",
                column: "GuestEmail");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_PaymentStatus",
                table: "Reservations",
                column: "PaymentStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_RoomId",
                table: "Reservations",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_Status",
                table: "Reservations",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_TenantId",
                table: "Reservations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Reservations_TenantId_ReservationNumber",
                table: "Reservations",
                columns: new[] { "TenantId", "ReservationNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_RolePermissions_PermissionId",
                table: "RolePermissions",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_BranchId",
                table: "Rooms",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_IsAvailable",
                table: "Rooms",
                column: "IsAvailable");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_RoomType",
                table: "Rooms",
                column: "RoomType");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_Status",
                table: "Rooms",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_TenantId",
                table: "Rooms",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Rooms_TenantId_RoomNumber",
                table: "Rooms",
                columns: new[] { "TenantId", "RoomNumber" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdminAuditLogs_Action",
                table: "SuperAdminAuditLogs",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdminAuditLogs_SuperAdminId",
                table: "SuperAdminAuditLogs",
                column: "SuperAdminId");

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdminAuditLogs_SuperAdminId_Timestamp",
                table: "SuperAdminAuditLogs",
                columns: new[] { "SuperAdminId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdminAuditLogs_TargetType",
                table: "SuperAdminAuditLogs",
                column: "TargetType");

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdminAuditLogs_TenantId",
                table: "SuperAdminAuditLogs",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdminAuditLogs_TenantId_Timestamp",
                table: "SuperAdminAuditLogs",
                columns: new[] { "TenantId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_SuperAdminAuditLogs_Timestamp",
                table: "SuperAdminAuditLogs",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "IX_TenantFeatures_FeatureName",
                table: "TenantFeatures",
                column: "FeatureName");

            migrationBuilder.CreateIndex(
                name: "IX_TenantFeatures_TenantId_FeatureName",
                table: "TenantFeatures",
                columns: new[] { "TenantId", "FeatureName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Country",
                table: "Tenants",
                column: "Country");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Identifier",
                table: "Tenants",
                column: "Identifier",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_IsActive",
                table: "Tenants",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_LicenseStatus",
                table: "Tenants",
                column: "LicenseStatus");

            migrationBuilder.CreateIndex(
                name: "IX_Tenants_Name",
                table: "Tenants",
                column: "Name");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "AuditLogDetails");

            migrationBuilder.DropTable(
                name: "PlanFeatures");

            migrationBuilder.DropTable(
                name: "PlanLimits");

            migrationBuilder.DropTable(
                name: "Reservations");

            migrationBuilder.DropTable(
                name: "RolePermissions");

            migrationBuilder.DropTable(
                name: "SuperAdminAuditLogs");

            migrationBuilder.DropTable(
                name: "TenantFeatures");

            migrationBuilder.DropTable(
                name: "AuditLogs");

            migrationBuilder.DropTable(
                name: "Plans");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "AspNetUsers");

            migrationBuilder.DropTable(
                name: "Branches");

            migrationBuilder.DropTable(
                name: "Tenants");
        }
    }
}
