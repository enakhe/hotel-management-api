namespace HotelManagement.Domain.Enums;

public enum LicenseStatusType
{
    Active,
    Expired,
    Suspended,
    Revoked,
    Trial
}

public enum LicenseType
{
    Standard,
    Premium,
    Enterprise,
    Trial,
    Custom
}

public enum LicenseKeyFormat
{
    XXXX_XXXX_XXXX_XXXX,
    XXXX_XXXX_XXXX_XXXX_XXXX,
    XXXX_XXXX_XXXX_XXXX_XXXX_XXXX
}
