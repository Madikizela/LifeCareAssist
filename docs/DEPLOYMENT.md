# Deployment Guide

## Local Development

### Prerequisites
- .NET 8.0 SDK
- PostgreSQL 14+
- Node.js 18+

### Quick Start
```powershell
# Run setup script
.\setup.ps1

# Update connection string in appsettings.json
# Then run migrations
dotnet ef migrations add InitialCreate --project src/RuralHealthcare.Infrastructure --startup-project src/RuralHealthcare.Web
dotnet ef database update --project src/RuralHealthcare.Infrastructure --startup-project src/RuralHealthcare.Web

# Run application
dotnet run --project src/RuralHealthcare.Web
```

## Production Deployment

### Option 1: Azure App Service

#### 1. Create Resources
```bash
# Create resource group
az group create --name rural-healthcare-rg --location southafricanorth

# Create PostgreSQL server
az postgres flexible-server create \
  --resource-group rural-healthcare-rg \
  --name rural-healthcare-db \
  --location southafricanorth \
  --admin-user dbadmin \
  --admin-password <secure-password> \
  --sku-name Standard_B1ms

# Create App Service
az appservice plan create \
  --name rural-healthcare-plan \
  --resource-group rural-healthcare-rg \
  --sku B1 \
  --is-linux

az webapp create \
  --resource-group rural-healthcare-rg \
  --plan rural-healthcare-plan \
  --name rural-healthcare-app \
  --runtime "DOTNET|8.0"
```

#### 2. Configure Connection String
```bash
az webapp config connection-string set \
  --resource-group rural-healthcare-rg \
  --name rural-healthcare-app \
  --settings DefaultConnection="Host=rural-healthcare-db.postgres.database.azure.com;Database=ruralhealthcare;Username=dbadmin;Password=<password>" \
  --connection-string-type PostgreSQL
```

#### 3. Deploy
```bash
# Publish application
dotnet publish src/RuralHealthcare.Web -c Release -o ./publish

# Deploy to Azure
az webapp deployment source config-zip \
  --resource-group rural-healthcare-rg \
  --name rural-healthcare-app \
  --src publish.zip
```

### Option 2: Docker

#### Dockerfile
```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/RuralHealthcare.Web/RuralHealthcare.Web.csproj", "RuralHealthcare.Web/"]
COPY ["src/RuralHealthcare.Core/RuralHealthcare.Core.csproj", "RuralHealthcare.Core/"]
COPY ["src/RuralHealthcare.Infrastructure/RuralHealthcare.Infrastructure.csproj", "RuralHealthcare.Infrastructure/"]
COPY ["src/RuralHealthcare.Services/RuralHealthcare.Services.csproj", "RuralHealthcare.Services/"]
RUN dotnet restore "RuralHealthcare.Web/RuralHealthcare.Web.csproj"
COPY src/ .
WORKDIR "/src/RuralHealthcare.Web"
RUN dotnet build "RuralHealthcare.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RuralHealthcare.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "RuralHealthcare.Web.dll"]
```

#### docker-compose.yml
```yaml
version: '3.8'

services:
  web:
    build: .
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - ConnectionStrings__DefaultConnection=Host=db;Database=ruralhealthcare;Username=postgres;Password=postgres
    depends_on:
      - db

  db:
    image: postgres:14
    environment:
      - POSTGRES_DB=ruralhealthcare
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=postgres
    volumes:
      - postgres_data:/var/lib/postgresql/data
    ports:
      - "5432:5432"

volumes:
  postgres_data:
```

### Option 3: Linux VPS (DigitalOcean, Linode)

#### 1. Setup Server
```bash
# Update system
sudo apt update && sudo apt upgrade -y

# Install .NET 8.0
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 8.0

# Install PostgreSQL
sudo apt install postgresql postgresql-contrib -y

# Install Nginx
sudo apt install nginx -y
```

#### 2. Configure Nginx
```nginx
server {
    listen 80;
    server_name yourdomain.com;

    location / {
        proxy_pass http://localhost:5000;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

#### 3. Setup Systemd Service
```ini
[Unit]
Description=Rural Healthcare Platform
After=network.target

[Service]
WorkingDirectory=/var/www/ruralhealthcare
ExecStart=/usr/bin/dotnet /var/www/ruralhealthcare/RuralHealthcare.Web.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
SyslogIdentifier=ruralhealthcare
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

## Environment Variables

### Required
```
ConnectionStrings__DefaultConnection=<postgres-connection-string>
```

### Optional
```
SmsProvider__Provider=Clickatell
SmsProvider__ApiKey=<your-api-key>
ASPNETCORE_ENVIRONMENT=Production
```

## SSL/TLS Certificate

### Using Let's Encrypt
```bash
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d yourdomain.com
```

## Monitoring

### Application Insights (Azure)
```csharp
builder.Services.AddApplicationInsightsTelemetry();
```

### Health Checks
```csharp
builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString)
    .AddCheck("sms-service", () => HealthCheckResult.Healthy());

app.MapHealthChecks("/health");
```

## Backup Strategy

### Database Backups
```bash
# Daily backup script
pg_dump -U postgres ruralhealthcare > backup_$(date +%Y%m%d).sql

# Upload to cloud storage
az storage blob upload --account-name backups --container-name db-backups --file backup_$(date +%Y%m%d).sql
```

### Automated Backups (Azure)
- Enable automated backups in Azure PostgreSQL
- Retention: 30 days minimum
- Geo-redundant backup for disaster recovery

## Scaling Considerations

### Horizontal Scaling
- Use Azure App Service scaling
- Load balancer for multiple instances
- Session state in Redis

### Database Scaling
- Read replicas for reporting
- Connection pooling
- Query optimization

### CDN
- Static assets on Azure CDN
- Reduce server load
- Improve global performance
