using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.DTOs;

// Request DTOs
public record CreateLicenseRequest
{
    public Guid PlanId { get; init; }
    public LicenseType Type { get; init; }
    public DateTime ExpirationDate { get; init; }
    public int? MaxValidations { get; init; }
    public string? HardwareId { get; init; }
    public string[]? DomainRestrictions { get; init; }
    public string[]? IpRestrictions { get; init; }
    public CreateLicenseFeatureRequest[]? Features { get; init; }
    public CreateLicenseLimitsRequest? Limits { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
    public string? CustomLicenseKey { get; init; }
}

public record CreateLicenseFeatureRequest
{
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool Enabled { get; init; }
    public int? Limit { get; init; }
    public string? Unit { get; init; }
    public Dictionary<string, object>? Configuration { get; init; }
}

public record CreateLicenseLimitsRequest
{
    public int MaxUsers { get; init; }
    public int MaxBranches { get; init; }
    public int MaxRooms { get; init; }
    public int MaxReservations { get; init; }
    public int MaxStorageGB { get; init; }
    public int ApiRateLimit { get; init; }
    public int ConcurrentSessions { get; init; }
    public Dictionary<string, int>? CustomLimits { get; init; }
}

public record UpdateLicenseRequest
{
    public LicenseStatusType? Status { get; init; }
    public DateTime? ExpirationDate { get; init; }
    public int? MaxValidations { get; init; }
    public string? HardwareId { get; init; }
    public string[]? DomainRestrictions { get; init; }
    public string[]? IpRestrictions { get; init; }
    public UpdateLicenseFeatureRequest[]? Features { get; init; }
    public UpdateLicenseLimitsRequest? Limits { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
}

public record UpdateLicenseFeatureRequest
{
    public string? Name { get; init; }
    public string? Description { get; init; }
    public bool? Enabled { get; init; }
    public int? Limit { get; init; }
    public string? Unit { get; init; }
    public Dictionary<string, object>? Configuration { get; init; }
}

public record UpdateLicenseLimitsRequest
{
    public int? MaxUsers { get; init; }
    public int? MaxBranches { get; init; }
    public int? MaxRooms { get; init; }
    public int? MaxReservations { get; init; }
    public int? MaxStorageGB { get; init; }
    public int? ApiRateLimit { get; init; }
    public int? ConcurrentSessions { get; init; }
    public Dictionary<string, int>? CustomLimits { get; init; }
}

public record LicenseListRequest
{
    public string? Query { get; init; }
    public LicenseStatusType? Status { get; init; }
    public LicenseType? Type { get; init; }
    public Guid? TenantId { get; init; }
    public Guid? PlanId { get; init; }
    public bool? Expired { get; init; }
    public int? ExpiresInDays { get; init; }
    public int Page { get; init; } = 1;
    public int Size { get; init; } = 10;
    public string SortBy { get; init; } = "created";
    public bool SortDescending { get; init; } = true;
}

// Response DTOs
public record LicenseResponseDto
{
    public Guid Id { get; init; }
    public string LicenseKey { get; init; } = string.Empty;
    public Guid TenantId { get; init; }
    public Guid PlanId { get; init; }
    public string? TenantName { get; init; }
    public string? PlanName { get; init; }
    public LicenseStatusType Status { get; init; }
    public LicenseType Type { get; init; }
    public DateTime IssuedDate { get; init; }
    public DateTime ExpirationDate { get; init; }
    public DateTime? LastValidated { get; init; }
    public int ValidationCount { get; init; }
    public int? MaxValidations { get; init; }
    public string? HardwareId { get; init; }
    public string[]? DomainRestrictions { get; init; }
    public string[]? IpRestrictions { get; init; }
    public LicenseFeatureResponseDto[] Features { get; init; } = Array.Empty<LicenseFeatureResponseDto>();
    public LicenseLimitsResponseDto? Limits { get; init; }
    public Dictionary<string, object>? Metadata { get; init; }
    public DateTime Created { get; init; }
    public string CreatedBy { get; init; } = string.Empty;
    public DateTime LastModified { get; init; }
    public string? LastModifiedBy { get; init; }
}

public record LicenseFeatureResponseDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Description { get; init; } = string.Empty;
    public bool Enabled { get; init; }
    public int? Limit { get; init; }
    public string? Unit { get; init; }
    public Dictionary<string, object>? Configuration { get; init; }
}

public record LicenseLimitsResponseDto
{
    public Guid Id { get; init; }
    public int MaxUsers { get; init; }
    public int MaxBranches { get; init; }
    public int MaxRooms { get; init; }
    public int MaxReservations { get; init; }
    public int MaxStorageGB { get; init; }
    public int ApiRateLimit { get; init; }
    public int ConcurrentSessions { get; init; }
    public Dictionary<string, int>? CustomLimits { get; init; }
}

// Validation DTOs
public record LicenseValidationRequest
{
    public string LicenseKey { get; init; } = string.Empty;
    public string? HardwareId { get; init; }
    public string? Domain { get; init; }
    public string? IpAddress { get; init; }
    public string? ClientVersion { get; init; }
    public Guid? TenantId { get; init; }
}

public record LicenseValidationResponse
{
    public bool IsValid { get; init; }
    public LicenseStatusType Status { get; init; }
    public DateTime ExpirationDate { get; init; }
    public int DaysUntilExpiration { get; init; }
    public LicenseFeatureResponseDto[] Features { get; init; } = Array.Empty<LicenseFeatureResponseDto>();
    public LicenseLimitsResponseDto? Limits { get; init; }
    public LicenseRestrictionsResponse Restrictions { get; init; } = new();
    public Dictionary<string, object>? Metadata { get; init; }
    public string ValidationId { get; init; } = string.Empty;
    public DateTime ValidatedAt { get; init; }
}

public record LicenseRestrictionsResponse
{
    public string? HardwareId { get; init; }
    public string[]? DomainRestrictions { get; init; }
    public string[]? IpRestrictions { get; init; }
}

// Management DTOs
public record LicenseRenewalRequest
{
    public Guid LicenseId { get; init; }
    public DateTime NewExpirationDate { get; init; }
    public string Reason { get; init; } = string.Empty;
    public CreateLicenseFeatureRequest[]? ExtendFeatures { get; init; }
    public CreateLicenseLimitsRequest? ExtendLimits { get; init; }
}

public record LicenseTransferRequest
{
    public Guid LicenseId { get; init; }
    public Guid NewTenantId { get; init; }
    public string Reason { get; init; } = string.Empty;
    public DateTime TransferDate { get; init; }
}

// Analytics DTOs
public record LicenseAnalyticsDto
{
    public int TotalLicenses { get; init; }
    public int ActiveLicenses { get; init; }
    public int ExpiredLicenses { get; init; }
    public int SuspendedLicenses { get; init; }
    public int RevokedLicenses { get; init; }
    public int TrialLicenses { get; init; }
    public int LicensesExpiringIn30Days { get; init; }
    public int LicensesExpiringIn7Days { get; init; }
    public decimal AverageValidationCount { get; init; }
    public TopValidatedLicenseDto[] TopValidatedLicenses { get; init; } = Array.Empty<TopValidatedLicenseDto>();
    public LicenseTypeDistributionDto[] LicenseTypeDistribution { get; init; } = Array.Empty<LicenseTypeDistributionDto>();
    public MonthlyIssuedLicenseDto[] MonthlyIssuedLicenses { get; init; } = Array.Empty<MonthlyIssuedLicenseDto>();
    public RevenueByLicenseTypeDto[] RevenueByLicenseType { get; init; } = Array.Empty<RevenueByLicenseTypeDto>();
}

public record TopValidatedLicenseDto
{
    public Guid LicenseId { get; init; }
    public string LicenseKey { get; init; } = string.Empty;
    public string TenantName { get; init; } = string.Empty;
    public int ValidationCount { get; init; }
}

public record LicenseTypeDistributionDto
{
    public LicenseType Type { get; init; }
    public int Count { get; init; }
    public decimal Percentage { get; init; }
}

public record MonthlyIssuedLicenseDto
{
    public string Month { get; init; } = string.Empty;
    public int Count { get; init; }
}

public record RevenueByLicenseTypeDto
{
    public LicenseType Type { get; init; }
    public decimal Revenue { get; init; }
    public string Currency { get; init; } = string.Empty;
}

public record LicenseUsageDto
{
    public Guid LicenseId { get; init; }
    public string LicenseKey { get; init; } = string.Empty;
    public string TenantName { get; init; } = string.Empty;
    public string PlanName { get; init; } = string.Empty;
    public LicenseStatusType Status { get; init; }
    public DateTime IssuedDate { get; init; }
    public DateTime ExpirationDate { get; init; }
    public int DaysUntilExpiration { get; init; }
    public int ValidationCount { get; init; }
    public int? MaxValidations { get; init; }
    public DateTime? LastValidated { get; init; }
    public LicenseCurrentUsageDto CurrentUsage { get; init; } = new();
    public LicenseLimitsResponseDto? Limits { get; init; }
    public LicenseUtilizationDto UtilizationPercentage { get; init; } = new();
}

public record LicenseCurrentUsageDto
{
    public int Users { get; init; }
    public int Branches { get; init; }
    public int Rooms { get; init; }
    public int Reservations { get; init; }
    public int StorageGB { get; init; }
    public int ApiCallsLast24h { get; init; }
}

public record LicenseUtilizationDto
{
    public decimal Users { get; init; }
    public decimal Branches { get; init; }
    public decimal Rooms { get; init; }
    public decimal Reservations { get; init; }
    public decimal Storage { get; init; }
}

// Key Generation DTOs
public record LicenseKeyGenerationOptions
{
    public LicenseKeyFormat Format { get; init; }
    public string? Prefix { get; init; }
    public string? Suffix { get; init; }
    public bool IncludeChecksum { get; init; }
    public string? CustomAlphabet { get; init; }
}

// Bulk Operation DTOs
public record BulkLicenseUpdateRequest
{
    public Guid Id { get; init; }
    public required UpdateLicenseRequest Data { get; init; }
}

public record BulkLicenseRenewalRequest
{
    public Guid LicenseId { get; init; }
    public DateTime NewExpirationDate { get; init; }
    public string Reason { get; init; } = string.Empty;
}

public record BulkLicenseSuspensionRequest
{
    public Guid LicenseId { get; init; }
    public string Reason { get; init; } = string.Empty;
}
