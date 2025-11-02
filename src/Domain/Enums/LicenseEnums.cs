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
    Standard = 0,
    Premium = 1,
    Enterprise = 2,
    Trial = 3,
    Custom = 4
}

public enum LicenseKeyFormat
{
    XXXX_XXXX_XXXX_XXXX,
    XXXX_XXXX_XXXX_XXXX_XXXX,
    XXXX_XXXX_XXXX_XXXX_XXXX_XXXX
}
