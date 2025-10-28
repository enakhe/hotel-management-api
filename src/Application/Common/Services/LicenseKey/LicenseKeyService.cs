using HotelManagement.Application.Common.Interfaces;
using HotelManagement.Application.Common.Models;
using HotelManagement.Domain.Enums;

namespace HotelManagement.Application.Common.Services.LicenseKey;

/// <summary>
/// License key service implementation
/// </summary>
public class LicenseKeyService : ILicenseKeyService
{
    public Task<Result<string>> GenerateLicenseKeyAsync(LicenseKeyGenerationOptions options)
    {
        try
        {
            var key = LicenseKeyGeneratorService.Generate(options);
            return Task.FromResult(Result<string>.Success(key, 200));
        }
        catch (ArgumentException ex)
        {
            return Task.FromResult(Result<string>.Failure(ex.Message, 400));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<string>.Failure($"An error occurred while generating the license key: {ex.Message}", 500));
        }
    }

    public Task<Result<LicenseKeyValidationResult>> ValidateLicenseKeyFormatAsync(string key, LicenseKeyFormat format, bool includeChecksum = true)
    {
        try
        {
            var result = LicenseKeyGeneratorService.ValidateDetailed(key, format, includeChecksum);
            return Task.FromResult(Result<LicenseKeyValidationResult>.Success(result, 200));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<LicenseKeyValidationResult>.Failure($"An error occurred while validating the license key: {ex.Message}", 500));
        }
    }

    public Task<Result<bool>> IsLicenseKeyUniqueAsync(string key)
    {
        try
        {
            // This would typically check against a database
            // For now, we'll assume it's unique if it passes format validation
            var isValid = LicenseKeyGeneratorService.Validate(key, LicenseKeyFormat.XXXX_XXXX_XXXX_XXXX, true);
            return Task.FromResult(Result<bool>.Success(isValid, 200));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<bool>.Failure($"An error occurred while checking license key uniqueness: {ex.Message}", 500));
        }
    }

    public Task<Result<string>> MaskLicenseKeyForDisplayAsync(string key)
    {
        try
        {
            var maskedKey = LicenseKeyGeneratorService.MaskForDisplay(key);
            return Task.FromResult(Result<string>.Success(maskedKey, 200));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<string>.Failure($"An error occurred while masking the license key: {ex.Message}", 500));
        }
    }

    public Task<Result<string>> FormatLicenseKeyForDisplayAsync(string key)
    {
        try
        {
            var formattedKey = LicenseKeyGeneratorService.FormatForDisplay(key);
            return Task.FromResult(Result<string>.Success(formattedKey, 200));
        }
        catch (Exception ex)
        {
            return Task.FromResult(Result<string>.Failure($"An error occurred while formatting the license key: {ex.Message}", 500));
        }
    }
}
