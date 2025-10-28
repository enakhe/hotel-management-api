using System.Text.Json.Serialization;

namespace HotelManagement.Domain.Entities;

public class License : BaseTenantEntity
{
    public string LicenseKey { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public Guid PlanId { get; set; }
    public LicenseStatusType Status { get; set; }
    public LicenseType Type { get; set; }
    public DateTime IssuedDate { get; set; }
    public DateTime ExpirationDate { get; set; }
    public DateTime? LastValidated { get; set; }
    public int ValidationCount { get; set; }
    public int? MaxValidations { get; set; }
    public string? HardwareId { get; set; }
    public string? DomainRestrictions { get; set; } // JSON array
    public string? IpRestrictions { get; set; } // JSON array
    public string? Metadata { get; set; } // JSON object
    public string CreatedBy { get; set; } = string.Empty;
    public string? LastModifiedBy { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual Plan Plan { get; set; } = null!;

    [JsonIgnore]
    public virtual ICollection<LicenseValidation> Validations { get; set; } = new List<LicenseValidation>();
}

public class LicenseValidation : BaseTenantEntity
{
    public Guid LicenseId { get; set; }
    public string ValidationId { get; set; } = string.Empty;
    public bool IsValid { get; set; }
    public string? HardwareId { get; set; }
    public string? Domain { get; set; }
    public string? IpAddress { get; set; }
    public string? ClientVersion { get; set; }
    public string? ValidationResult { get; set; } // JSON object
    public string? FailureReason { get; set; }

    // Navigation properties
    [JsonIgnore]
    public virtual License License { get; set; } = null!;
}
