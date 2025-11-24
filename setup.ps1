# Rural Healthcare Platform - Setup Script for Windows

Write-Host "🏥 Rural Healthcare Platform Setup" -ForegroundColor Green
Write-Host "====================================`n" -ForegroundColor Green

# Check .NET SDK
Write-Host "Checking .NET SDK..." -ForegroundColor Yellow
$dotnetVersion = dotnet --version
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ .NET SDK $dotnetVersion found" -ForegroundColor Green
} else {
    Write-Host "✗ .NET SDK not found. Please install .NET 8.0 SDK" -ForegroundColor Red
    exit 1
}

# Check Node.js
Write-Host "`nChecking Node.js..." -ForegroundColor Yellow
$nodeVersion = node --version
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Node.js $nodeVersion found" -ForegroundColor Green
} else {
    Write-Host "✗ Node.js not found. Please install Node.js 18+" -ForegroundColor Red
    exit 1
}

# Restore NuGet packages
Write-Host "`nRestoring NuGet packages..." -ForegroundColor Yellow
dotnet restore
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ NuGet packages restored" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to restore packages" -ForegroundColor Red
    exit 1
}

# Install npm packages
Write-Host "`nInstalling Tailwind CSS..." -ForegroundColor Yellow
Set-Location src/RuralHealthcare.Web
npm install
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Tailwind CSS installed" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to install npm packages" -ForegroundColor Red
    exit 1
}

# Build Tailwind CSS
Write-Host "`nBuilding Tailwind CSS..." -ForegroundColor Yellow
npm run build:css
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ Tailwind CSS built" -ForegroundColor Green
} else {
    Write-Host "✗ Failed to build CSS" -ForegroundColor Red
}

Set-Location ../..

Write-Host "`n====================================`n" -ForegroundColor Green
Write-Host "✓ Setup complete!" -ForegroundColor Green
Write-Host "`nNext steps:" -ForegroundColor Yellow
Write-Host "1. Setup PostgreSQL database (see docs/SETUP.md)" -ForegroundColor White
Write-Host "2. Update connection string in src/RuralHealthcare.Web/appsettings.json" -ForegroundColor White
Write-Host "3. Run migrations:" -ForegroundColor White
Write-Host "   dotnet ef migrations add InitialCreate --project src/RuralHealthcare.Infrastructure --startup-project src/RuralHealthcare.Web" -ForegroundColor Cyan
Write-Host "   dotnet ef database update --project src/RuralHealthcare.Infrastructure --startup-project src/RuralHealthcare.Web" -ForegroundColor Cyan
Write-Host "4. Run the application:" -ForegroundColor White
Write-Host "   dotnet run --project src/RuralHealthcare.Web" -ForegroundColor Cyan
Write-Host "`n"
