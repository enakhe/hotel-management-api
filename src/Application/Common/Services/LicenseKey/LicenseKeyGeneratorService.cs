using System.Security.Cryptography;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.Services.LicenseKey;

/// <summary>
/// Enterprise-grade license key generation service
/// Handles cryptographically secure license key generation
/// </summary>
public class LicenseKeyGeneratorService
{
    private static readonly string DEFAULT_ALPHABET = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private static readonly string CHECKSUM_ALPHABET = "0123456789ABCDEF";

    /// <summary>
    /// Generates a cryptographically secure license key
    /// </summary>
    public static string Generate(LicenseKeyGenerationOptions options)
    {
        var format = options.Format;
        var prefix = options.Prefix ?? string.Empty;
        var suffix = options.Suffix ?? string.Empty;
        var includeChecksum = options.IncludeChecksum;
        var customAlphabet = options.CustomAlphabet ?? DEFAULT_ALPHABET;

        // Validate alphabet
        if (customAlphabet.Length < 10)
            throw new ArgumentException("Custom alphabet must have at least 10 characters");

        // Determine the number of segments and characters per segment
        var segmentInfo = GetSegmentInfo(format);

        var key = prefix;

        // Generate segments
        for (int i = 0; i < segmentInfo.Segments; i++)
        {
            if (i > 0) key += "-";

            // Generate random characters for this segment
            for (int j = 0; j < segmentInfo.CharsPerSegment; j++)
            {
                var randomIndex = GetSecureRandomInt(0, customAlphabet.Length - 1);
                key += customAlphabet[randomIndex];
            }
        }

        key += suffix;

        // Add checksum if requested
        if (includeChecksum)
        {
            var checksum = CalculateChecksum(key);
            key += $"-{checksum}";
        }

        return key;
    }

    /// <summary>
    /// Validates a license key format and checksum
    /// </summary>
    public static bool Validate(string key, LicenseKeyFormat expectedFormat, bool includeChecksum = true)
    {
        try
        {
            var segmentInfo = GetSegmentInfo(expectedFormat);
            var segments = key.Split('-');

            // Check segment count
            var expectedSegments = segmentInfo.Segments + (includeChecksum ? 1 : 0);
            if (segments.Length != expectedSegments)
                return false;

            // Check each segment length
            for (int i = 0; i < segmentInfo.Segments; i++)
            {
                if (segments[i].Length != segmentInfo.CharsPerSegment)
                    return false;
            }

            // Validate checksum if present
            if (includeChecksum)
            {
                var keyWithoutChecksum = string.Join("-", segments.Take(segmentInfo.Segments));
                var providedChecksum = segments[segmentInfo.Segments];
                var calculatedChecksum = CalculateChecksum(keyWithoutChecksum);

                if (providedChecksum != calculatedChecksum)
                    return false;
            }

            return true;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Masks a license key for display (shows only first and last segments)
    /// </summary>
    public static string MaskForDisplay(string key)
    {
        var segments = key.Split('-');
        if (segments.Length <= 2)
        {
            return new string('*', key.Length);
        }

        var firstSegment = segments[0];
        var lastSegment = segments[segments.Length - 1];
        var maskedMiddle = segments.Skip(1).Take(segments.Length - 2)
            .Select(segment => new string('*', segment.Length));

        return string.Join("-", new[] { firstSegment }.Concat(maskedMiddle).Concat(new[] { lastSegment }));
    }

    /// <summary>
    /// Formats a license key for better readability
    /// </summary>
    public static string FormatForDisplay(string key)
    {
        return key.ToUpper().Replace("-", " - ");
    }

    /// <summary>
    /// Validates license key format and returns detailed validation result
    /// </summary>
    public static LicenseKeyValidationResult ValidateDetailed(string key, LicenseKeyFormat expectedFormat, bool includeChecksum = true)
    {
        var errors = new List<string>();
        var warnings = new List<string>();

        if (string.IsNullOrWhiteSpace(key))
        {
            errors.Add("License key cannot be empty");
            return new LicenseKeyValidationResult { IsValid = false, Errors = errors, Warnings = warnings };
        }

        var isValid = Validate(key, expectedFormat, includeChecksum);

        if (!isValid)
        {
            errors.Add("Invalid license key format");
        }

        // Check for common issues
        if (key.Contains(" "))
        {
            warnings.Add("License key contains spaces");
        }

        if (key != key.ToUpper())
        {
            warnings.Add("License key should be uppercase");
        }

        return new LicenseKeyValidationResult { IsValid = isValid, Errors = errors, Warnings = warnings };
    }

    private static SegmentInfo GetSegmentInfo(LicenseKeyFormat format)
    {
        return format switch
        {
            LicenseKeyFormat.XXXX_XXXX_XXXX_XXXX => new SegmentInfo { Segments = 4, CharsPerSegment = 4 },
            LicenseKeyFormat.XXXX_XXXX_XXXX_XXXX_XXXX => new SegmentInfo { Segments = 5, CharsPerSegment = 4 },
            LicenseKeyFormat.XXXX_XXXX_XXXX_XXXX_XXXX_XXXX => new SegmentInfo { Segments = 6, CharsPerSegment = 4 },
            _ => new SegmentInfo { Segments = 4, CharsPerSegment = 4 }
        };
    }

    private static string CalculateChecksum(string key)
    {
        // Calculate checksum using the checksum alphabet
        int checksum = 0;
        foreach (char c in key)
        {
            checksum += c;
        }

        // Convert to checksum alphabet characters
        var checksumChars = new char[4];
        for (int i = 0; i < 4; i++)
        {
            checksumChars[i] = CHECKSUM_ALPHABET[checksum % CHECKSUM_ALPHABET.Length];
            checksum /= CHECKSUM_ALPHABET.Length;
        }

        return new string(checksumChars);
    }

    private static int GetSecureRandomInt(int min, int max)
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        var randomInt = BitConverter.ToUInt32(bytes, 0);
        return min + (int)(randomInt % (max - min + 1));
    }

    private record SegmentInfo
    {
        public int Segments { get; init; }
        public int CharsPerSegment { get; init; }
    }
}

/// <summary>
/// License key generation options
/// </summary>
public record LicenseKeyGenerationOptions
{
    public LicenseKeyFormat Format { get; init; }
    public string? Prefix { get; init; }
    public string? Suffix { get; init; }
    public bool IncludeChecksum { get; init; } = true;
    public string? CustomAlphabet { get; init; }
}

/// <summary>
/// License key validation result
/// </summary>
public record LicenseKeyValidationResult
{
    public bool IsValid { get; init; }
    public List<string> Errors { get; init; } = new();
    public List<string> Warnings { get; init; } = new();
}
