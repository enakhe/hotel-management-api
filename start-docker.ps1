# PowerShell script to start Docker containers and apply migrations

Write-Host "Starting Docker containers..." -ForegroundColor Green
docker-compose up -d

Write-Host "Waiting for SQL Server to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 30

Write-Host "Applying database migrations..." -ForegroundColor Green
cd src/Infrastructure
dotnet ef database update --startup-project ../Web

Write-Host "Database setup complete!" -ForegroundColor Green
Write-Host "You can now run the application with: dotnet run --project src/Web" -ForegroundColor Cyan
