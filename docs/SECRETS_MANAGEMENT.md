# Secrets Management Guide

## Overview

This application uses different strategies for managing secrets across environments:
- **Development**: .NET User Secrets
- **Production**: Azure Key Vault
- **CI/CD**: Environment variables or Key Vault references

## Development Setup (User Secrets)

### Initial Configuration

User Secrets has been initialized for the Web project. The UserSecretsId is stored in `Web.csproj`.

### Setting Up Your Local Environment

Run the following commands from the `src/Web` directory:

```bash
# JWT Configuration
dotnet user-secrets set "Jwt:Key" "YOUR-SECRET-JWT-KEY-HERE-MIN-32-CHARS"

# Database Connection
dotnet user-secrets set "ConnectionStrings:sql" "Server=localhost;Database=HotelManagementDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;MultipleActiveResultSets=true;TrustServerCertificate=True"

# Redis Cache Connection
dotnet user-secrets set "ConnectionStrings:cache" "localhost:6379"

# Email Configuration
dotnet user-secrets set "Email:Password" "YOUR-EMAIL-PASSWORD"
```

### Viewing Current Secrets

```bash
cd src/Web
dotnet user-secrets list
```

### Removing a Secret

```bash
dotnet user-secrets remove "SecretKey"
```

### Clearing All Secrets

```bash
dotnet user-secrets clear
```

## Production Setup (Azure Key Vault)

### Prerequisites

1. Azure subscription
2. Azure Key Vault resource created
3. Managed Identity or Service Principal configured

### Key Vault Setup

#### 1. Create Key Vault

```bash
# Set variables
RESOURCE_GROUP="rg-hotelmanagement-prod"
KEY_VAULT_NAME="kv-hotelmanagement-prod"
LOCATION="eastus"

# Create resource group
az group create --name $RESOURCE_GROUP --location $LOCATION

# Create Key Vault
az keyvault create \
  --name $KEY_VAULT_NAME \
  --resource-group $RESOURCE_GROUP \
  --location $LOCATION \
  --enable-rbac-authorization true
```

#### 2. Add Secrets to Key Vault

```bash
# Add JWT Key
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "Jwt--Key" \
  --value "YOUR-PRODUCTION-JWT-KEY"

# Add SQL Connection String
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "ConnectionStrings--sql" \
  --value "YOUR-PRODUCTION-SQL-CONNECTION"

# Add Redis Connection String
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "ConnectionStrings--cache" \
  --value "YOUR-PRODUCTION-REDIS-CONNECTION"

# Add Email Password
az keyvault secret set \
  --vault-name $KEY_VAULT_NAME \
  --name "Email--Password" \
  --value "YOUR-PRODUCTION-EMAIL-PASSWORD"
```

**Note**: Azure Key Vault uses `--` as the separator instead of `:` in secret names.

#### 3. Configure Managed Identity

##### For App Service:

```bash
# Enable system-assigned managed identity
az webapp identity assign \
  --name your-app-service-name \
  --resource-group $RESOURCE_GROUP

# Grant access to Key Vault
PRINCIPAL_ID=$(az webapp identity show \
  --name your-app-service-name \
  --resource-group $RESOURCE_GROUP \
  --query principalId -o tsv)

az role assignment create \
  --role "Key Vault Secrets User" \
  --assignee $PRINCIPAL_ID \
  --scope /subscriptions/{subscription-id}/resourceGroups/$RESOURCE_GROUP/providers/Microsoft.KeyVault/vaults/$KEY_VAULT_NAME
```

#### 4. Configure Application

The application is already configured to use Key Vault when `KeyVaultUri` is set:

```json
{
  "KeyVaultUri": "https://kv-hotelmanagement-prod.vault.azure.net/"
}
```

Add this to your App Service Application Settings:

```bash
az webapp config appsettings set \
  --name your-app-service-name \
  --resource-group $RESOURCE_GROUP \
  --settings KeyVaultUri="https://$KEY_VAULT_NAME.vault.azure.net/"
```

## CI/CD Configuration

### GitHub Actions

```yaml
- name: Azure Login
  uses: azure/login@v1
  with:
    creds: ${{ secrets.AZURE_CREDENTIALS }}

- name: Set Key Vault URI
  run: |
    echo "KeyVaultUri=https://kv-hotelmanagement-prod.vault.azure.net/" >> $GITHUB_ENV
```

### Azure DevOps

1. Create a Variable Group linked to Key Vault
2. Reference secrets in pipeline:

```yaml
variables:
  - group: hotelmanagement-keyvault

steps:
- task: AzureWebApp@1
  inputs:
    appSettings: |
      -KeyVaultUri "$(KeyVaultUri)"
```

## Security Best Practices

### 1. JWT Key Generation

Generate a strong JWT key (minimum 256 bits):

```bash
# PowerShell
$bytes = New-Object byte[] 32
[System.Security.Cryptography.RandomNumberGenerator]::Fill($bytes)
[System.Convert]::ToBase64String($bytes)

# Linux/Mac
openssl rand -base64 32
```

### 2. Connection String Security

- **Never commit connection strings** to source control
- Use separate databases for dev/staging/prod
- Rotate credentials regularly
- Use Azure SQL Managed Identity when possible

### 3. Secrets Rotation

Regular rotation schedule:
- **JWT Keys**: Every 90 days
- **Database Passwords**: Every 90 days
- **Email Passwords**: Every 90 days
- **API Keys**: Every 30-90 days

### 4. Access Control

Key Vault RBAC roles:
- **Key Vault Administrator**: Full management (ops team only)
- **Key Vault Secrets User**: Read secrets (application identity)
- **Key Vault Secrets Officer**: Manage secrets (DevOps pipelines)

## Troubleshooting

### User Secrets Not Found

If secrets aren't loading in development:

1. Verify UserSecretsId in `Web.csproj`
2. Check secrets location:
   - **Windows**: `%APPDATA%\Microsoft\UserSecrets\{UserSecretsId}\secrets.json`
   - **Linux/Mac**: `~/.microsoft/usersecrets/{UserSecretsId}/secrets.json`
3. Ensure secrets are set: `dotnet user-secrets list`

### Key Vault Access Denied

1. Verify Managed Identity is enabled
2. Check RBAC role assignments
3. Ensure Key Vault URI is correct
4. Check firewall rules (if enabled)

### Connection String Issues

If application can't connect to database:

```bash
# Test connection string
sqlcmd -S server-name -d database-name -U username -P password
```

## Required Secrets Reference

| Secret Key | Purpose | Example Value | Required |
|------------|---------|---------------|----------|
| `Jwt:Key` | JWT token signing | `base64-encoded-32-byte-key` | Yes |
| `ConnectionStrings:sql` | SQL Server | `Server=...;Database=...` | Yes |
| `ConnectionStrings:cache` | Redis Cache | `localhost:6379` | Yes |
| `Email:Password` | SMTP Auth | `email-password` | Yes |
| `KeyVaultUri` | Key Vault URL | `https://kv-name.vault.azure.net/` | Prod only |

## Environment-Specific Configuration

### Development
- Use User Secrets for all sensitive data
- Local SQL Server with Windows Authentication
- Local Redis instance

### Staging
- Use Key Vault with service principal
- Azure SQL Database with AD authentication
- Azure Redis Cache

### Production
- Use Key Vault with Managed Identity
- Azure SQL Database with AD authentication (recommended)
- Azure Redis Cache with private endpoint
- Enable Key Vault soft delete and purge protection

## Migration from Old Configuration

If upgrading from hardcoded secrets:

1. Extract all secrets from `appsettings.json`
2. Add to User Secrets (dev) or Key Vault (prod)
3. Replace with placeholder messages
4. Test application startup
5. Commit sanitized `appsettings.json`

## Monitoring and Auditing

- Enable Key Vault diagnostic logging
- Monitor access patterns in Azure Monitor
- Set up alerts for unusual access
- Regular access reviews for Key Vault

## Additional Resources

- [.NET User Secrets Documentation](https://docs.microsoft.com/aspnet/core/security/app-secrets)
- [Azure Key Vault Documentation](https://docs.microsoft.com/azure/key-vault/)
- [Managed Identities for Azure Resources](https://docs.microsoft.com/azure/active-directory/managed-identities-azure-resources/)

