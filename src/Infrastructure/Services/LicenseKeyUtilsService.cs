using HotelManagement.Domain.Enums;
using HotelManagement.Application.Common.Services.LicenseKey;

namespace HotelManagement.Infrastructure.Services.LicenseKey;

/// <summary>
/// Utility service for license key operations
/// </summary>
public class LicenseKeyUtilsService
{
    /// <summary>
    /// Checks if a license key is expired based on expiration date
    /// </summary>
    public static bool IsExpired(string key, DateTime expirationDate)
    {
        var now = DateTime.UtcNow;
        return now > expirationDate;
    }

    /// <summary>
    /// Calculates days until expiration
    /// </summary>
    public static int GetDaysUntilExpiration(DateTime expirationDate)
    {
        var now = DateTime.UtcNow;
        var diffTime = expirationDate - now;
        return (int)Math.Ceiling(diffTime.TotalDays);
    }

    /// <summary>
    /// Generates a license key with embedded expiration date
    /// </summary>
    public static string GenerateWithExpiration(LicenseKeyGenerationOptions options, DateTime expirationDate)
    {
        var key = LicenseKeyGeneratorService.Generate(options);
        // In a real implementation, you might encode the expiration date in the key
        return key;
    }

    /// <summary>
    /// Generates multiple unique license keys
    /// </summary>
    public static List<string> GenerateMultiple(LicenseKeyGenerationOptions options, int count)
    {
        var keys = new List<string>();
        var generatedKeys = new HashSet<string>();

        for (int i = 0; i < count; i++)
        {
            string key;
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                key = LicenseKeyGeneratorService.Generate(options);
                attempts++;
            }
            while (generatedKeys.Contains(key) && attempts < maxAttempts);

            if (attempts >= maxAttempts)
                throw new InvalidOperationException($"Failed to generate unique key after {maxAttempts} attempts");

            generatedKeys.Add(key);
            keys.Add(key);
        }

        return keys;
    }

    /// <summary>
    /// Extracts metadata from a license key (if encoded)
    /// </summary>
    public static LicenseKeyMetadata ExtractMetadata(string key)
    {
        // This is a placeholder for extracting encoded metadata
        // In a real implementation, you might encode metadata in the key
        return new LicenseKeyMetadata();
    }

    /// <summary>
    /// Generates a license key with embedded metadata
    /// </summary>
    public static string GenerateWithMetadata(LicenseKeyGenerationOptions options, LicenseKeyMetadata metadata)
    {
        var key = LicenseKeyGeneratorService.Generate(options);
        // In a real implementation, you might encode metadata in the key
        return key;
    }

    /// <summary>
    /// Validates license key uniqueness against existing keys
    /// </summary>
    public static bool IsUnique(string key, IEnumerable<string> existingKeys)
    {
        return !existingKeys.Contains(key, StringComparer.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Generates a license key with specific patterns
    /// </summary>
    public static string GenerateWithPattern(LicenseKeyFormat format, string pattern)
    {
        var options = new LicenseKeyGenerationOptions
        {
            Format = format,
            IncludeChecksum = true
        };

        var key = LicenseKeyGeneratorService.Generate(options);

        // Apply pattern if specified
        if (!string.IsNullOrEmpty(pattern))
        {
            // This is a simplified pattern application
            // In a real implementation, you might have more complex pattern logic
            key = ApplyPattern(key, pattern);
        }

        return key;
    }

    private static string ApplyPattern(string key, string pattern)
    {
        // Simple pattern application - replace X with random characters
        var result = new System.Text.StringBuilder();
        var random = new Random();
        var alphabet = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        foreach (char c in pattern)
        {
            if (c == 'X')
            {
                result.Append(alphabet[random.Next(alphabet.Length)]);
            }
            else
            {
                result.Append(c);
            }
        }

        return result.ToString();
    }
}

/// <summary>
/// License key metadata
/// </summary>
public record LicenseKeyMetadata
{
    public string? TenantId { get; init; }
    public string? PlanId { get; init; }
    public DateTime? IssuedDate { get; init; }
    public string? Version { get; init; }
    public Dictionary<string, object>? CustomData { get; init; }
}
