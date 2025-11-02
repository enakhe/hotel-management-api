<!-- 3f290cea-058a-4a18-b3e2-ce57e3d907b0 ade16bda-fb3c-4a4e-a0c7-b76f7c81bcbc -->
# SuperAdmin Report Management Feature

## Overview

Build a comprehensive reporting system for SuperAdmin control plane (/cp) that provides system-wide insights across all tenants, licenses, plans, and operations. The system will support multiple report types, export formats, scheduled generation, and email delivery.

## Architecture Approach

Following the existing Clean Architecture pattern with CQRS:

- **Domain Layer**: Report entities, enums, and value objects
- **Application Layer**: Report queries/commands, DTOs, and interfaces
- **Infrastructure Layer**: Report services, export generators, background jobs, email service
- **Web Layer**: ReportManagementController under `/cp/reports`

## Report Categories

### 1. System Overview Reports

- Tenants overview (active, inactive, by region, by plan)
- Licenses overview (active, expired, expiring soon)
- Plans & modules adoption metrics
- System health dashboard

### 2. Financial Reports

- Subscription revenue by period
- License sales analytics
- Revenue by plan/region/tenant
- Payment trends and forecasts

### 3. Tenant-Specific Reports

- Tenant usage metrics (users, branches, reservations, rooms)
- Tenant activity timelines
- Resource utilization vs limits
- Tenant growth trends

### 4. Audit & Compliance Reports

- SuperAdmin audit logs
- Tenant audit logs (cross-tenant view)
- Security events and anomalies
- Compliance tracking

### 5. Analytics Reports

- User growth trends across all tenants
- Module adoption rates
- License utilization patterns
- Peak usage analysis

## API Endpoints Design

### Report Generation Endpoints

```
GET    /cp/reports                          - List all reports with filters
GET    /cp/reports/types                    - Get available report types
GET    /cp/reports/{reportId}               - Get specific report details
POST   /cp/reports/generate                 - Generate on-demand report
GET    /cp/reports/{reportId}/download      - Download generated report
DELETE /cp/reports/{reportId}               - Delete report (archive)

GET    /cp/reports/templates                - List report templates
POST   /cp/reports/templates                - Create custom report template
PUT    /cp/reports/templates/{templateId}   - Update report template
DELETE /cp/reports/templates/{templateId}   - Delete report template
```

### Scheduled Reports Endpoints

```
GET    /cp/reports/schedules                - List scheduled reports
POST   /cp/reports/schedules                - Create report schedule
GET    /cp/reports/schedules/{scheduleId}   - Get schedule details
PUT    /cp/reports/schedules/{scheduleId}   - Update schedule
DELETE /cp/reports/schedules/{scheduleId}   - Delete schedule
POST   /cp/reports/schedules/{scheduleId}/pause    - Pause schedule
POST   /cp/reports/schedules/{scheduleId}/resume   - Resume schedule
```

### Report Subscriptions Endpoints

```
GET    /cp/reports/subscriptions            - List email subscriptions
POST   /cp/reports/subscriptions            - Subscribe to report
DELETE /cp/reports/subscriptions/{id}       - Unsubscribe
```

### Analytics & History Endpoints

```
GET    /cp/reports/history                  - Report generation history
GET    /cp/reports/analytics                - Report system analytics
GET    /cp/reports/popular                  - Most generated reports
```

## Key Components to Build

### 1. Domain Layer (`src/Domain/Entities`)

**New Entities:**

- `Report` - Generated report metadata
- `ReportSchedule` - Scheduled report configuration
- `ReportSubscription` - Email subscription to reports
- `ReportTemplate` - Custom report templates
- `ReportExecution` - Report generation execution log

**New Enums:**

- `ReportType` - System, Financial, Tenant, Audit, Analytics
- `ReportFormat` - PDF, Excel, CSV, JSON, HTML
- `ReportStatus` - Pending, Processing, Completed, Failed
- `ScheduleFrequency` - Daily, Weekly, Monthly, Quarterly
- `ReportCategory` - Categorization for organization

### 2. Application Layer

**DTOs (`src/Application/Common/DTOs`):**

- `ReportRequestDto` - Report generation request
- `ReportResponseDto` - Report metadata response
- `ReportScheduleDto` - Schedule configuration
- `ReportSubscriptionDto` - Email subscription
- `ReportTemplateDto` - Custom template configuration
- `ReportDataDto` - Actual report data structure
- `ReportFilterDto` - Filter parameters

**Queries (`src/Application/Core/ReportManagement/Queries`):**

- `GetReportsQuery` - List reports with pagination/filtering
- `GetReportByIdQuery` - Get specific report
- `GetReportTypesQuery` - List available report types
- `GetReportTemplatesQuery` - List report templates
- `GetScheduledReportsQuery` - List scheduled reports
- `GetReportHistoryQuery` - Report generation history
- `GetReportAnalyticsQuery` - Report system analytics

**Commands (`src/Application/Core/ReportManagement/Commands`):**

- `GenerateReportCommand` - Generate on-demand report
- `CreateReportScheduleCommand` - Create schedule
- `UpdateReportScheduleCommand` - Update schedule
- `DeleteReportScheduleCommand` - Delete schedule
- `CreateReportSubscriptionCommand` - Subscribe to report
- `DeleteReportSubscriptionCommand` - Unsubscribe
- `CreateReportTemplateCommand` - Create custom template
- `DeleteReportCommand` - Delete/archive report

**Interfaces (`src/Application/Common/Interfaces`):**

- `IReportService` - Main report service interface
- `IReportGeneratorService` - Report generation logic
- `IReportExportService` - Export format handlers
- `IReportSchedulerService` - Scheduling logic
- `IEmailService` - Email delivery service
- `IBackgroundJobService` - Background job management

### 3. Infrastructure Layer

**Services (`src/Infrastructure/Services`):**

- `ReportService` - Main report orchestration
- `ReportGeneratorService` - Report data generation
- `ReportExportService` - Export format conversion
- `PdfReportExporter` - PDF generation
- `ExcelReportExporter` - Excel generation
- `CsvReportExporter` - CSV generation
- `JsonReportExporter` - JSON generation
- `HtmlReportExporter` - HTML generation
- `EmailService` - Email delivery
- `BackgroundJobService` - Background job scheduling (using Hangfire or similar)

**Report Generators (Strategy Pattern):**

- `SystemOverviewReportGenerator`
- `FinancialReportGenerator`
- `TenantUsageReportGenerator`
- `AuditReportGenerator`
- `AnalyticsReportGenerator`

**Data Configurations (`src/Infrastructure/Data/Configurations`):**

- `ReportConfiguration`
- `ReportScheduleConfiguration`
- `ReportSubscriptionConfiguration`
- `ReportTemplateConfiguration`

### 4. Web Layer

**Controller (`src/Web/Controllers/SuperAdmin`):**

- `ReportManagementController` - All report endpoints

**Middleware (if needed):**

- Report file streaming middleware for large downloads

## Technology Stack Additions

### Required NuGet Packages

1. **PDF Generation:**

   - QuestPDF or iTextSharp

2. **Excel Generation:**

   - EPPlus or ClosedXML

3. **Background Jobs:**

   - Hangfire or Quartz.NET

4. **Email Service:**

   - MailKit or SendGrid SDK

5. **CSV:**

   - CsvHelper

6. **HTML Templates:**

   - RazorLight or Scriban

## Database Schema Additions

### Reports Table

- Id, Type, Category, Name, Description, Format, Status
- GeneratedBy (SuperAdminId), GeneratedAt, CompletedAt
- FilePath, FileSize, DownloadUrl, ExpiresAt
- Parameters (JSON), Filters (JSON)
- TenantId (nullable - for tenant-specific reports)
- ErrorMessage (for failed reports)

### ReportSchedules Table

- Id, Name, ReportType, Format, Frequency
- CronExpression, NextRunAt, LastRunAt
- IsActive, Parameters (JSON), Filters (JSON)
- EmailRecipients (JSON array)
- CreatedBy, CreatedAt, UpdatedAt

### ReportSubscriptions Table

- Id, SuperAdminId, Email, ReportScheduleId
- IsActive, CreatedAt, UnsubscribeToken

### ReportTemplates Table

- Id, Name, Description, ReportType
- TemplateConfig (JSON), CustomFields (JSON)
- IsPublic, CreatedBy, CreatedAt, UpdatedAt

### ReportExecutions Table

- Id, ReportScheduleId, ReportId
- Status, StartedAt, CompletedAt, Duration
- RecordCount, ErrorMessage

## Implementation Strategy

### Phase 1: Core Infrastructure

1. Add domain entities and enums
2. Setup database configurations and migrations
3. Implement base report service interfaces
4. Add background job infrastructure (Hangfire)
5. Implement email service

### Phase 2: Report Generators

1. Implement report generator factory/strategy
2. Create individual report generators for each type
3. Build data aggregation logic
4. Add caching for frequently accessed data

### Phase 3: Export Services

1. Implement export service interface
2. Create format-specific exporters (PDF, Excel, CSV, JSON, HTML)
3. Add file storage strategy (local/cloud)
4. Implement download URL generation

### Phase 4: API & Controllers

1. Create ReportManagementController
2. Implement on-demand report generation endpoints
3. Add report listing and download endpoints
4. Implement SuperAdmin authorization

### Phase 5: Scheduling

1. Implement report scheduling service
2. Create schedule management commands/queries
3. Add Hangfire background jobs
4. Implement email delivery for scheduled reports

### Phase 6: Advanced Features

1. Report subscriptions
2. Custom report templates
3. Report analytics and history
4. Performance optimization and caching

## Security Considerations

- All endpoints under `[Authorize(Roles = "SuperAdmin")]`
- Sensitive reports require `[Authorize(Policy = "RequireStepUpAuth")]`
- Report download URLs are time-limited and signed
- Audit logging for all report generation
- Rate limiting on report generation to prevent abuse
- File size limits on exports
- Automatic cleanup of old reports

## Performance Optimizations

- Cache frequently requested report data (Redis)
- Async report generation for large datasets
- Background jobs for scheduled reports
- Pagination for large result sets
- Lazy loading of report data
- Database query optimization with projections
- File compression for large exports

## Testing Strategy

- Unit tests for report generators
- Integration tests for export services
- Functional tests for API endpoints
- Performance tests for large dataset reports
- Email delivery tests (mock SMTP)

## File Organization

```
src/
├── Domain/
│   ├── Entities/
│   │   ├── Report.cs
│   │   ├── ReportSchedule.cs
│   │   ├── ReportSubscription.cs
│   │   ├── ReportTemplate.cs
│   │   └── ReportExecution.cs
│   └── Enums/
│       ├── ReportType.cs
│       ├── ReportFormat.cs
│       ├── ReportStatus.cs
│       └── ScheduleFrequency.cs
├── Application/
│   ├── Common/
│   │   ├── DTOs/
│   │   │   ├── Report/
│   │   │   │   ├── ReportRequestDto.cs
│   │   │   │   ├── ReportResponseDto.cs
│   │   │   │   ├── ReportScheduleDto.cs
│   │   │   │   ├── ReportSubscriptionDto.cs
│   │   │   │   └── ReportTemplateDto.cs
│   │   └── Interfaces/
│   │       ├── IReportService.cs
│   │       ├── IReportGeneratorService.cs
│   │       ├── IReportExportService.cs
│   │       ├── IReportSchedulerService.cs
│   │       ├── IEmailService.cs
│   │       └── IBackgroundJobService.cs
│   └── Core/
│       └── ReportManagement/
│           ├── Commands/
│           │   ├── GenerateReport.cs
│           │   ├── CreateReportSchedule.cs
│           │   ├── UpdateReportSchedule.cs
│           │   ├── DeleteReportSchedule.cs
│           │   ├── CreateReportSubscription.cs
│           │   └── DeleteReportSubscription.cs
│           └── Queries/
│               ├── GetReports.cs
│               ├── GetReportById.cs
│               ├── GetReportTypes.cs
│               ├── GetReportSchedules.cs
│               └── GetReportHistory.cs
├── Infrastructure/
│   ├── Data/
│   │   └── Configurations/
│   │       ├── ReportConfiguration.cs
│   │       ├── ReportScheduleConfiguration.cs
│   │       └── ReportSubscriptionConfiguration.cs
│   └── Services/
│       ├── Reports/
│       │   ├── ReportService.cs
│       │   ├── ReportGeneratorService.cs
│       │   ├── ReportExportService.cs
│       │   ├── Generators/
│       │   │   ├── IReportDataGenerator.cs
│       │   │   ├── SystemOverviewReportGenerator.cs
│       │   │   ├── FinancialReportGenerator.cs
│       │   │   ├── TenantUsageReportGenerator.cs
│       │   │   ├── AuditReportGenerator.cs
│       │   │   └── AnalyticsReportGenerator.cs
│       │   └── Exporters/
│       │       ├── IReportExporter.cs
│       │       ├── PdfReportExporter.cs
│       │       ├── ExcelReportExporter.cs
│       │       ├── CsvReportExporter.cs
│       │       ├── JsonReportExporter.cs
│       │       └── HtmlReportExporter.cs
│       ├── EmailService.cs
│       └── BackgroundJobService.cs
└── Web/
    └── Controllers/
        └── SuperAdmin/
            └── ReportManagementController.cs
```

### To-dos

- [ ] Setup core infrastructure: Add NuGet packages (Hangfire, MailKit, EPPlus, QuestPDF, CsvHelper), create domain entities and enums
- [ ] Create database configurations and migrations for Report, ReportSchedule, ReportSubscription, ReportTemplate, ReportExecution entities
- [ ] Define service interfaces and DTOs for report management in Application layer
- [ ] Implement EmailService and BackgroundJobService infrastructure
- [ ] Build report data generators (System Overview, Financial, Tenant Usage, Audit, Analytics)
- [ ] Implement export services for all formats (PDF, Excel, CSV, JSON, HTML)
- [ ] Implement main ReportService orchestration layer
- [ ] Create CQRS queries and commands for report management
- [ ] Build ReportManagementController with all API endpoints
- [ ] Implement report scheduling with Hangfire background jobs and email delivery
- [ ] Implement report subscriptions and email notifications
- [ ] Add custom report templates functionality
- [ ] Add security features (authorization policies, rate limiting, audit logging) and performance optimizations (caching, compression)
- [ ] Write unit tests, integration tests, and functional tests for report system