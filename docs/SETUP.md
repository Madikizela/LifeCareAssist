# Setup Guide

## Prerequisites
1. .NET 8.0 SDK - [Download](https://dotnet.microsoft.com/download)
2. PostgreSQL 14+ - [Download](https://www.postgresql.org/download/)
3. Node.js 18+ - [Download](https://nodejs.org/)
4. Visual Studio 2022 or VS Code

## Database Setup

### 1. Install PostgreSQL
```bash
# Windows: Download installer from postgresql.org
# Or use Chocolatey
choco install postgresql
```

### 2. Create Database
```sql
CREATE DATABASE ruralhealthcare;
CREATE USER healthcare_user WITH PASSWORD 'your_secure_password';
GRANT ALL PRIVILEGES ON DATABASE ruralhealthcare TO healthcare_user;
```

### 3. Update Connection String
Edit `src/RuralHealthcare.Web/appsettings.json`:
```json
"ConnectionStrings": {
  "DefaultConnection": "Host=localhost;Database=ruralhealthcare;Username=healthcare_user;Password=your_secure_password"
}
```

## Application Setup

### 1. Restore NuGet Packages
```bash
dotnet restore
```

### 2. Install Tailwind CSS
```bash
cd src/RuralHealthcare.Web
npm install
```

### 3. Build Tailwind CSS
```bash
npm run build:css
```

### 4. Run Migrations
```bash
dotnet ef migrations add InitialCreate --project src/RuralHealthcare.Infrastructure --startup-project src/RuralHealthcare.Web
dotnet ef database update --project src/RuralHealthcare.Infrastructure --startup-project src/RuralHealthcare.Web
```

### 5. Run Application
```bash
dotnet run --project src/RuralHealthcare.Web
```

Visit: https://localhost:5001

## SMS Provider Setup (Optional)

### Clickatell (Recommended for SA)
1. Sign up at https://www.clickatell.com/
2. Get API key
3. Update appsettings.json

### Twilio (Alternative)
1. Sign up at https://www.twilio.com/
2. Get Account SID and Auth Token
3. Update appsettings.json

## Development Workflow

### Watch Tailwind CSS Changes
```bash
cd src/RuralHealthcare.Web
npm run watch:css
```

### Run with Hot Reload
```bash
dotnet watch --project src/RuralHealthcare.Web
```
