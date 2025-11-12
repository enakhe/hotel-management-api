using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HotelManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCompleteBillingSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BasePrice",
                table: "Plans",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountPercentage",
                table: "Plans",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PricingStrategy",
                table: "Plans",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "SetupFee",
                table: "Plans",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxEmailsPerMonth",
                table: "Limits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxSmsPerMonth",
                table: "Limits",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Priority = table.Column<int>(type: "int", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ActionUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Data = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ExpiresAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRead = table.Column<bool>(type: "bit", nullable: false),
                    IsDelivered = table.Column<bool>(type: "bit", nullable: false),
                    IsArchived = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TemplateKey = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Category = table.Column<int>(type: "int", nullable: false),
                    EmailSubject = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    EmailBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    SmsBody = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PushTitle = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    PushBody = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    InAppBody = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Variables = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Language = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    IsSystem = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationTemplates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "OveragePricings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Metric = table.Column<int>(type: "int", nullable: false),
                    PricePerUnit = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    MinimumBillableUnit = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    UpdatedBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OveragePricings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OveragePricings_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Subscriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TrialEndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    BillingCycle = table.Column<int>(type: "int", nullable: false),
                    MonthlyPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    CustomDiscount = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    DiscountReason = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    NextBillingDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastBillingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AutoRenew = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Subscriptions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Subscriptions_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TenantOnboardings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CurrentStep = table.Column<int>(type: "int", nullable: false),
                    ContactPerson = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactEmail = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContactPhone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    StartedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumberOfRooms = table.Column<int>(type: "int", nullable: true),
                    SelectedPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CustomizationData = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TenantOnboardings", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TenantOnboardings_Plans_SelectedPlanId",
                        column: x => x.SelectedPlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_TenantOnboardings_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserNotificationPreferences",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Category = table.Column<int>(type: "int", nullable: false),
                    EmailEnabled = table.Column<bool>(type: "bit", nullable: false),
                    SmsEnabled = table.Column<bool>(type: "bit", nullable: false),
                    PushEnabled = table.Column<bool>(type: "bit", nullable: false),
                    InAppEnabled = table.Column<bool>(type: "bit", nullable: false),
                    QuietHoursStart = table.Column<TimeSpan>(type: "time", nullable: true),
                    QuietHoursEnd = table.Column<TimeSpan>(type: "time", nullable: true),
                    PreferredLanguage = table.Column<string>(type: "nvarchar(10)", maxLength: 10, nullable: true),
                    DigestEnabled = table.Column<bool>(type: "bit", nullable: false),
                    DigestFrequency = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserNotificationPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserNotificationPreferences_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "NotificationDeliveries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    NotificationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Channel = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeliveredAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ErrorMessage = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RetryCount = table.Column<int>(type: "int", nullable: false),
                    MaxRetries = table.Column<int>(type: "int", nullable: false),
                    ExternalId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NotificationDeliveries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_NotificationDeliveries_Notifications_NotificationId",
                        column: x => x.NotificationId,
                        principalTable: "Notifications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Invoices",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    IssueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    DueDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    Subtotal = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    DiscountAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountPaid = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    AmountDue = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    PeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PaymentTermsDays = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PaymentInstructions = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    SentAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ViewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Invoices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Invoices_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Invoices_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SubscriptionModules",
                columns: table => new
                {
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsOptional = table.Column<bool>(type: "bit", nullable: false),
                    AddedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RemovedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SubscriptionModules", x => new { x.SubscriptionId, x.ModuleId });
                    table.ForeignKey(
                        name: "FK_SubscriptionModules_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SubscriptionModules_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "InvoiceLineItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LineNumber = table.Column<int>(type: "int", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Type = table.Column<int>(type: "int", nullable: false),
                    ModuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    UnitPrice = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsTaxable = table.Column<bool>(type: "bit", nullable: false),
                    TaxRate = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    TaxAmount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    ServicePeriodStart = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ServicePeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InvoiceLineItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_InvoiceLineItems_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InvoiceLineItems_Modules_ModuleId",
                        column: x => x.ModuleId,
                        principalTable: "Modules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_InvoiceLineItems_Plans_PlanId",
                        column: x => x.PlanId,
                        principalTable: "Plans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "PaymentRetries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AttemptNumber = table.Column<int>(type: "int", nullable: false),
                    AttemptedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    NextRetryAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<int>(type: "int", nullable: false),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    PaymentMethod = table.Column<int>(type: "int", nullable: true),
                    NotificationSent = table.Column<bool>(type: "bit", nullable: false),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PaymentRetries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PaymentRetries_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PaymentRetries_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PaymentReference = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: false),
                    Method = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ProcessedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    SettledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    GatewayTransactionId = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    GatewayReference = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    Gateway = table.Column<int>(type: "int", nullable: false),
                    GatewayResponse = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PaymentMethodDetails = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    Last4Digits = table.Column<string>(type: "nvarchar(4)", maxLength: 4, nullable: true),
                    CardBrand = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    FailureReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    IsReconciled = table.Column<bool>(type: "bit", nullable: false),
                    ReconciledAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReconciledBy = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsageAggregations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    BillingPeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BillingPeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EmailsSent = table.Column<int>(type: "int", nullable: false),
                    EmailsIncluded = table.Column<int>(type: "int", nullable: false),
                    EmailsOverage = table.Column<int>(type: "int", nullable: false),
                    EmailOverageCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    SmsSent = table.Column<int>(type: "int", nullable: false),
                    SmsIncluded = table.Column<int>(type: "int", nullable: false),
                    SmsOverage = table.Column<int>(type: "int", nullable: false),
                    SmsOverageCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    StorageUsedGB = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StorageIncludedGB = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StorageOverageGB = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    StorageOverageCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    ApiCallsMade = table.Column<int>(type: "int", nullable: false),
                    ApiCallsIncluded = table.Column<int>(type: "int", nullable: false),
                    ApiCallsOverage = table.Column<int>(type: "int", nullable: false),
                    ApiCallsOverageCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    TotalOverageCost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: false),
                    IsFinalized = table.Column<bool>(type: "bit", nullable: false),
                    FinalizedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsBilled = table.Column<bool>(type: "bit", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageAggregations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsageAggregations_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UsageAggregations_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UsageAggregations_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UsageRecords",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SubscriptionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Metric = table.Column<int>(type: "int", nullable: false),
                    Quantity = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: false),
                    Cost = table.Column<decimal>(type: "decimal(18,2)", precision: 18, scale: 2, nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BillingPeriodStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    BillingPeriodEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsBilled = table.Column<bool>(type: "bit", nullable: false),
                    InvoiceId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Metadata = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    TenantId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UsageRecords", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UsageRecords_Invoices_InvoiceId",
                        column: x => x.InvoiceId,
                        principalTable: "Invoices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UsageRecords_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_UsageRecords_Tenants_TenantId",
                        column: x => x.TenantId,
                        principalTable: "Tenants",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_InvoiceId",
                table: "InvoiceLineItems",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_ModuleId",
                table: "InvoiceLineItems",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_PlanId",
                table: "InvoiceLineItems",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_InvoiceLineItems_Type",
                table: "InvoiceLineItems",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_DueDate",
                table: "Invoices",
                column: "DueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_InvoiceNumber",
                table: "Invoices",
                column: "InvoiceNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_IssueDate",
                table: "Invoices",
                column: "IssueDate");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_Status",
                table: "Invoices",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_SubscriptionId",
                table: "Invoices",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Invoices_TenantId",
                table: "Invoices",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveries_Channel",
                table: "NotificationDeliveries",
                column: "Channel");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveries_NotificationId",
                table: "NotificationDeliveries",
                column: "NotificationId");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveries_NotificationId_Channel",
                table: "NotificationDeliveries",
                columns: new[] { "NotificationId", "Channel" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationDeliveries_Status",
                table: "NotificationDeliveries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedAt",
                table: "Notifications",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_IsRead",
                table: "Notifications",
                column: "IsRead");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_TenantId",
                table: "Notifications",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Type",
                table: "Notifications",
                column: "Type");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_Category",
                table: "NotificationTemplates",
                column: "Category");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_IsActive",
                table: "NotificationTemplates",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_IsSystem",
                table: "NotificationTemplates",
                column: "IsSystem");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_TemplateKey",
                table: "NotificationTemplates",
                column: "TemplateKey");

            migrationBuilder.CreateIndex(
                name: "IX_NotificationTemplates_TenantId_TemplateKey",
                table: "NotificationTemplates",
                columns: new[] { "TenantId", "TemplateKey" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OveragePricings_IsActive",
                table: "OveragePricings",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_OveragePricings_Metric",
                table: "OveragePricings",
                column: "Metric");

            migrationBuilder.CreateIndex(
                name: "IX_OveragePricings_PlanId",
                table: "OveragePricings",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRetries_AttemptedAt",
                table: "PaymentRetries",
                column: "AttemptedAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRetries_InvoiceId",
                table: "PaymentRetries",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRetries_NextRetryAt",
                table: "PaymentRetries",
                column: "NextRetryAt");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRetries_Status",
                table: "PaymentRetries",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_PaymentRetries_TenantId",
                table: "PaymentRetries",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CreatedAt",
                table: "Payments",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_GatewayTransactionId",
                table: "Payments",
                column: "GatewayTransactionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_InvoiceId",
                table: "Payments",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_PaymentReference",
                table: "Payments",
                column: "PaymentReference",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_Status",
                table: "Payments",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_SubscriptionId",
                table: "Payments",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_TenantId",
                table: "Payments",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionModules_AddedAt",
                table: "SubscriptionModules",
                column: "AddedAt");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionModules_ModuleId",
                table: "SubscriptionModules",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_SubscriptionModules_RemovedAt",
                table: "SubscriptionModules",
                column: "RemovedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_NextBillingDate",
                table: "Subscriptions",
                column: "NextBillingDate");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_PlanId",
                table: "Subscriptions",
                column: "PlanId");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_Status",
                table: "Subscriptions",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_SubscriptionNumber",
                table: "Subscriptions",
                column: "SubscriptionNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Subscriptions_TenantId",
                table: "Subscriptions",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantOnboardings_CurrentStep",
                table: "TenantOnboardings",
                column: "CurrentStep");

            migrationBuilder.CreateIndex(
                name: "IX_TenantOnboardings_SelectedPlanId",
                table: "TenantOnboardings",
                column: "SelectedPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_TenantOnboardings_StartedAt",
                table: "TenantOnboardings",
                column: "StartedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TenantOnboardings_Status",
                table: "TenantOnboardings",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_TenantOnboardings_TenantId",
                table: "TenantOnboardings",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageAggregations_BillingPeriodStart_BillingPeriodEnd",
                table: "UsageAggregations",
                columns: new[] { "BillingPeriodStart", "BillingPeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_UsageAggregations_InvoiceId",
                table: "UsageAggregations",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageAggregations_IsBilled",
                table: "UsageAggregations",
                column: "IsBilled");

            migrationBuilder.CreateIndex(
                name: "IX_UsageAggregations_IsFinalized",
                table: "UsageAggregations",
                column: "IsFinalized");

            migrationBuilder.CreateIndex(
                name: "IX_UsageAggregations_SubscriptionId",
                table: "UsageAggregations",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageAggregations_TenantId",
                table: "UsageAggregations",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageRecords_InvoiceId",
                table: "UsageRecords",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageRecords_IsBilled",
                table: "UsageRecords",
                column: "IsBilled");

            migrationBuilder.CreateIndex(
                name: "IX_UsageRecords_Metric",
                table: "UsageRecords",
                column: "Metric");

            migrationBuilder.CreateIndex(
                name: "IX_UsageRecords_RecordedAt",
                table: "UsageRecords",
                column: "RecordedAt");

            migrationBuilder.CreateIndex(
                name: "IX_UsageRecords_SubscriptionId",
                table: "UsageRecords",
                column: "SubscriptionId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageRecords_TenantId",
                table: "UsageRecords",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_UsageRecords_TenantId_BillingPeriodStart_BillingPeriodEnd",
                table: "UsageRecords",
                columns: new[] { "TenantId", "BillingPeriodStart", "BillingPeriodEnd" });

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreferences_UserId",
                table: "UserNotificationPreferences",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserNotificationPreferences_UserId_Category",
                table: "UserNotificationPreferences",
                columns: new[] { "UserId", "Category" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InvoiceLineItems");

            migrationBuilder.DropTable(
                name: "NotificationDeliveries");

            migrationBuilder.DropTable(
                name: "NotificationTemplates");

            migrationBuilder.DropTable(
                name: "OveragePricings");

            migrationBuilder.DropTable(
                name: "PaymentRetries");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "SubscriptionModules");

            migrationBuilder.DropTable(
                name: "TenantOnboardings");

            migrationBuilder.DropTable(
                name: "UsageAggregations");

            migrationBuilder.DropTable(
                name: "UsageRecords");

            migrationBuilder.DropTable(
                name: "UserNotificationPreferences");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "Invoices");

            migrationBuilder.DropTable(
                name: "Subscriptions");

            migrationBuilder.DropColumn(
                name: "BasePrice",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "DiscountPercentage",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "PricingStrategy",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "SetupFee",
                table: "Plans");

            migrationBuilder.DropColumn(
                name: "MaxEmailsPerMonth",
                table: "Limits");

            migrationBuilder.DropColumn(
                name: "MaxSmsPerMonth",
                table: "Limits");
        }
    }
}
