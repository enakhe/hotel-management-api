# Setup User Secrets for Development
# Run this script from the repository root

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "  Hotel Management - User Secrets Setup" -ForegroundColor Cyan
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

# Get script directory and navigate to repository root
$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot = Split-Path -Parent $scriptDir
$projectPath = Join-Path $repoRoot "src\Web"

# Navigate to project directory
Push-Location $projectPath

Write-Host "Setting up User Secrets for Web project..." -ForegroundColor Yellow
Write-Host ""

# Generate a secure JWT key
function Generate-JwtKey {
    $bytes = New-Object byte[] 32
    [System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
    return [System.Convert]::ToBase64String($bytes)
}

# Prompt for values or use defaults
Write-Host "Please provide the following configuration values:" -ForegroundColor Green
Write-Host "(Press Enter to use default values shown in brackets)" -ForegroundColor Gray
Write-Host ""

# JWT Key
$jwtKeyPrompt = Read-Host "JWT Signing Key [Generate new secure key]"
if ([string]::IsNullOrWhiteSpace($jwtKeyPrompt)) {
    $jwtKey = Generate-JwtKey
    Write-Host "  Generated secure JWT key" -ForegroundColor Gray
}
else {
    $jwtKey = $jwtKeyPrompt
}

# SQL Connection
Write-Host ""
$sqlServer = Read-Host "SQL Server [localhost]"
if ([string]::IsNullOrWhiteSpace($sqlServer)) { $sqlServer = "localhost" }

$sqlDatabase = Read-Host "Database Name [HotelManagementDb]"
if ([string]::IsNullOrWhiteSpace($sqlDatabase)) { $sqlDatabase = "HotelManagementDb" }

$sqlAuth = Read-Host "Use Windows Authentication? (y/n) [y]"
if ([string]::IsNullOrWhiteSpace($sqlAuth) -or $sqlAuth -eq "y") {
    $sqlConnection = "Server=$sqlServer;Database=$sqlDatabase;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True"
}
else {
    $sqlUser = Read-Host "SQL Username"
    $sqlPassword = Read-Host "SQL Password" -AsSecureString
    $sqlPasswordText = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($sqlPassword))
    $sqlConnection = "Server=$sqlServer;Database=$sqlDatabase;User Id=$sqlUser;Password=$sqlPasswordText;MultipleActiveResultSets=true;TrustServerCertificate=True"
}

# Redis Connection
Write-Host ""
$redisConnection = Read-Host "Redis Connection [localhost:6379]"
if ([string]::IsNullOrWhiteSpace($redisConnection)) { $redisConnection = "localhost:6379" }

# Email Configuration
Write-Host ""
Write-Host "Email Configuration (for password reset, notifications, etc.)" -ForegroundColor Yellow
$emailPassword = Read-Host "Email Password (App Password for Gmail)" -AsSecureString
$emailPasswordText = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($emailPassword))

Write-Host ""
Write-Host "Setting secrets..." -ForegroundColor Yellow

# Set secrets
dotnet user-secrets set "Jwt:Key" "$jwtKey" | Out-Null
dotnet user-secrets set "ConnectionStrings:sql" "$sqlConnection" | Out-Null
dotnet user-secrets set "ConnectionStrings:cache" "$redisConnection" | Out-Null
dotnet user-secrets set "Email:Password" "$emailPasswordText" | Out-Null

Write-Host ""
Write-Host "User Secrets configured successfully!" -ForegroundColor Green
Write-Host ""
Write-Host "Current secrets:" -ForegroundColor Cyan
dotnet user-secrets list

Write-Host ""
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "Setup complete! You can now run the application." -ForegroundColor Green
Write-Host ""
Write-Host "To view secrets: dotnet user-secrets list" -ForegroundColor Gray
Write-Host "To remove a secret: dotnet user-secrets remove SecretKey" -ForegroundColor Gray
Write-Host "To clear all secrets: dotnet user-secrets clear" -ForegroundColor Gray
Write-Host ""
Write-Host "See docs/SECRETS_MANAGEMENT.md for more information." -ForegroundColor Gray
Write-Host "===============================================" -ForegroundColor Cyan

Pop-Location
